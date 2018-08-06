using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace End2EndTest
{
    public partial class HubRpcTest
    {
        private interface IAssertInjector
        {
            Task<string> TestEcho(string message);
        }

        private class AssertInjector : IAssertInjector
        {
            private dynamic _methodInvoker;
            public AssertInjector(dynamic methodInvoker)
            {
                _methodInvoker = methodInvoker;
            }

            public async Task<string> TestEcho(string message)
            {
                return await _methodInvoker.TestEcho(message);
            }
        }
        
        private class MockHub : Hub, IAssertInjector
        {
            private readonly IAssertInjector _assertInjector;
            public MockHub(IAssertInjector assertInjector)
            {
                _assertInjector = assertInjector;
            }

            public async Task<string> TestEcho(string message)
            {
                return await _assertInjector.TestEcho(message);
            }
        }
    }
}