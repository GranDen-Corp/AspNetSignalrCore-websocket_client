using System;
using System.Threading.Tasks;
using AutoFixture;
using End2EndTest.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace End2EndTest
{
    public class HubRpcTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public HubRpcTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private class EchoHub : Hub
        {
#pragma warning disable CS1998
            // ReSharper disable once UnusedMember.Global
            public async Task<string> EchoWithJsonFormat(string message)
                // ReSharper restore UnusedMember.Global 
#pragma warning restore CS1998
            {
                var sendStr = $"{{\"recv\": \"{message}\"}}";

                return sendStr;
            }
        }

        [Fact]
        public async Task ClientConnectTestServerCanInvokeRpcAndGetResult()
        {
            const string clientAddress = "ws://localhost:54321/ws";
            const string serverAddress = "http://localhost:54321";
            var serverTestComplte = false;
            const string message = "Hello World";

            var config = AspNetCoreSignalRHelper.CreateConfigWithUrl(serverAddress);

            var configureService = new Action<IServiceCollection>(services =>
            {
                services.AddSignalR(hubOptions => { hubOptions.EnableDetailedErrors = true; });
            });

            var hubRouteBuilder = new Action<HubRouteBuilder>(builder =>
            {
                builder.CallMapHub(typeof(EchoHub),"/ws");
            });

            using (var server =
                AspNetCoreSignalRHelper.CreateTestServer(config, _testOutputHelper, configureService, hubRouteBuilder))
            {
                await server.StartAsync().OrTimeout();
                var connection = new HubConnectionBuilder()
                    .WithUrl($"{serverAddress}/ws")
                    .Build();

                await connection.StartAsync();
                var recv = await connection.InvokeAsync<string>("EchoWithJsonFormat", message);
                Assert.Equal($"{{\"recv\": \"{message}\"}}", recv);
            }
        }
    }
}