using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using UnityEngine;

namespace SerilogUnity3dSink
{
    public class SerilogUnity3dSink : ILogEventSink
    {
        private IFormatProvider _formatProvider;

        public SerilogUnity3dSink(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }
        
        public void Emit(LogEvent logEvent)
        {
            var msg = logEvent.RenderMessage();
            
            if (logEvent.Level == LogEventLevel.Error || logEvent.Level == LogEventLevel.Fatal)
            {
                Debug.LogError(msg);
            }
            else if (logEvent.Level == LogEventLevel.Warning)
            {
                Debug.LogWarning(msg);
            }
            else
            {
                Debug.Log(msg);
            }
        }
    }
    
    public static class SerilogUnity3dSinkExtensions
    {
        public static LoggerConfiguration UnityConsole(
            this LoggerSinkConfiguration loggerConfiguration,
            IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new SerilogUnity3dSink(formatProvider));
        }
    }
}