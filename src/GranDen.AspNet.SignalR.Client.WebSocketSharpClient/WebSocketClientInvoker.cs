
using System;
using WebSocketSharp;

namespace GranDen.AspNet.SignalR.Client.WebSocketSharpClient
{
    public class WebSocketClientInvoker
    {
        private WebSocket _webSocket;
        private Serilog.ILogger _logger;

        public WebSocketClientInvoker(string url)
        {
            _webSocket = new WebSocket(url) {EnableRedirection = true};
            _webSocket.Log 
        }

        ~WebSocketClientInvoker()
        {
            var socket = _webSocket as IDisposable;
            socket?.Dispose();
        }
        
        #region API Methods

        public void Connect()
        {
            _webSocket.Connect();
        }

        public void Close()
        {
            _webSocket.Close();
        }
        
       #endregion 
        
        
        
        

    }
}