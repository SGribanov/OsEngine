#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Diagnostics;

namespace OsEngine.Market.Servers.QuikLua.Entity
{
    public class CustomTraceListener : TraceListener
    {
        public override void Write(string message) { }

        public override void WriteLine(string message) { }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (eventType == TraceEventType.Error)
            {
                OnTraceMessageReceived?.Invoke(message);
            }
        }

        public static event Action<string> OnTraceMessageReceived;
    }
}

