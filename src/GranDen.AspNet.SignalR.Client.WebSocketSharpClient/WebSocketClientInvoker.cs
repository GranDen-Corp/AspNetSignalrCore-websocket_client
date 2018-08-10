
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
            _webSocket = new WebSocket(url, CreateLogger()) {EnableRedirection = true};
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


        private WebSocketSharp.Logger CreateLogger()
        {
            return new WebSocketSharp.Logger(WebSocketSharp.LogLevel.Debug, null, (logData, _) =>
            {
                _logger.Debug(logData.Message);
            });
        }
        
    }
}