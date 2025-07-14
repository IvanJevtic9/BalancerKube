using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace BalancerKube.Common.Telemetry;

public class SerilogOpenTelemetrySpanSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        if (Activity.Current != null)
        {
            var formattedMessage = logEvent.RenderMessage();
            var level = logEvent.Level.ToString();
            var eventText = $"[{level}] {formattedMessage}";

            // Add exception info if present
            if (logEvent.Exception != null)
            {
                eventText += $" Exception: {logEvent.Exception}";
            }

            Activity.Current.AddEvent(new ActivityEvent(eventText));
        }
    }
}

