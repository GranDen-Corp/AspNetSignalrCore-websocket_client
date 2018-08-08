using System;
using System.Dynamic;
using System.Threading.Tasks;
using End2EndTest.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace End2EndTest
{
    public partial class HubRpcTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public HubRpcTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task ClientConnectTestServerCanInvokeRpcAndGetResult()
        {
            const string serverAddress = "http://localhost:54321";
            var serverTestComplte = false;
            const string message = "Hello World";

            var config = AspNetCoreSignalRHelper.CreateConfigWithUrl(serverAddress);

            var configureService = new Action<IServiceCollection>(services =>
            {
                dynamic assertInjector = new ExpandoObject();
                assertInjector.TestEcho = new Func<string,Task<string> >(async (input) =>
                {
                    Assert.Equal(message, input);
                    var sendStr = $"{{\"recv\": \"{input}\"}}";
                    serverTestComplte = true;
                    return sendStr;
                });

                services.AddScoped<IAssertInjector>(provider =>  new AssertInjector(assertInjector));
                
                services.AddSignalR(hubOptions => { hubOptions.EnableDetailedErrors = true; });
            });

            var hubRouteBuilder = new Action<HubRouteBuilder>(builder =>
            {
                builder.CallMapHub(typeof(MockHub),"/ws");
            });

            using (var server =
                AspNetCoreSignalRHelper.CreateTestServer(config, _testOutputHelper, configureService, hubRouteBuilder))
            {
                await server.StartAsync().OrTimeout();
                var connection = new HubConnectionBuilder()
                    .WithUrl($"{serverAddress}/ws")
                    .Build();

                await connection.StartAsync().OrTimeout();
                var recv = await connection.InvokeAsync<string>("TestEcho", message).OrTimeout();
                Assert.Equal($"{{\"recv\": \"{message}\"}}", recv);
            }
            Assert.True(serverTestComplte, "server verification failed");
        }
    }
}