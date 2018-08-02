using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace End2EndTest.Utils
{
    public static class AspNetCoreSignalRHelper
    {
        public static IWebHost CreateTestServer(IConfiguration config, ITestOutputHelper testOutputHelper,
            Action<IServiceCollection> configService, Action<HubRouteBuilder> hubRouteBuilder, bool forceHttps = false)
        {
            
            Action<IApplicationBuilder> startup = builder =>
            {
                if (forceHttps)
                {
                    builder.UseHttpsRedirection();
                }

                builder.Use(async (context, next) =>
                {
                    try
                    {
                        await next.Invoke();
                    }
                    catch (Exception ex)
                    {
                        if (context.Response.HasStarted)
                        {
                            throw;
                        }

                        context.Response.StatusCode = 500;
                        context.Response.Headers.Clear();
                        await context.Response.WriteAsync(ex.ToString());
                    }
                });

                builder.UseSignalR(hubRouteBuilder);
            };

            var host = new WebHostBuilder()
                .ConfigureServices(s => s.AddSingleton(CreateLogFactory(testOutputHelper)))
                .UseConfiguration(config)
                .UseKestrel()
                .ConfigureServices(services =>
                    {
                        services.AddSingleton<IStartup>(serviceProvider => new DynamicStartup(configService, startup));
                    })
                .Build();

            return host;
        }
        
        public static IConfiguration CreateConfigWithUrl(string url)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection();
            var config = configBuilder.Build();
            config["server.urls"] = url;
            return config;
        }

        private static ILoggerFactory CreateLogFactory(ITestOutputHelper testOutputHelper)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
            return loggerFactory;
        }
    }

    public class DynamicStartup : StartupBase
    {
        private readonly Action<IServiceCollection> _configureService;
        private readonly Action<IApplicationBuilder> _configureApp;

        public DynamicStartup(Action<IServiceCollection> configureService, Action<IApplicationBuilder> configureApp)
        {
            _configureService = configureService;
            _configureApp = configureApp;
        }

        public override void ConfigureServices(IServiceCollection services) => _configureService(services);

        public override void Configure(IApplicationBuilder app) => _configureApp(app);

    }
}