using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace End2EndTest
{
    public partial class HubRpcTest
    {
        //for ImproptuInterface
        // ReSharper disable once MemberCanBePrivate.Global
        public interface IAssertInjector
        // ReSharper restore MemberCanBePrivate.Global
        {
            Task<string> TestEcho(string message);
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