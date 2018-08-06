using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
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
                .UseConfiguration(config)
                .UseKestrel()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IStartup>(serviceProvider => new DynamicStartup(configService, startup));
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddDebug();
                    logging.AddProvider(new XunitLoggerProvider(testOutputHelper));
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
            config["Logging:LogLevel:Microsoft.AspNetCore.SignalR"] = "Trace";
            config["Logging:LogLevel:Microsoft.AspNetCore.Http.Connections"] = "Debug";
            return config;
        }

        public static void CallMapHub(this HubRouteBuilder hubRouteBuilder, Type hubClassType, PathString pathString,
            Action<HttpConnectionDispatcherOptions> configureOptions = null)
        {
            Action<PathString, Action<HttpConnectionDispatcherOptions>> action = hubRouteBuilder.MapHub<Hub>;

            var mapHubMethodInfo = action.Method.GetGenericMethodDefinition().MakeGenericMethod(hubClassType);

            mapHubMethodInfo.Invoke(hubRouteBuilder, new object[] {pathString, configureOptions});
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

    public class StubHub : Hub
    {
    }
}