using Serilog.Core;
using Serilog.Events;
using System;

namespace ECommerce.API.Logging
{
    public class InMemoryLogSink(InMemoryLogBuffer buffer) : ILogEventSink
    {
        private readonly InMemoryLogBuffer _buffer = buffer;

        public void Emit(LogEvent logEvent)
        {
            // Skip noisy EF/MassTransit internals
            var ctx = logEvent.Properties.TryGetValue("SourceContext", out var sc)
                ? sc.ToString().Trim('"')
                : null;

            if (ctx != null && (
                ctx.StartsWith("Microsoft.EntityFrameworkCore") ||
                ctx.StartsWith("MassTransit") ||
                ctx.StartsWith("Microsoft.Hosting")))
                return;

            _buffer.Add(new LogEntry
            {
                Timestamp = logEvent.Timestamp,
                Level = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(),
                Exception = logEvent.Exception?.ToString(),
                SourceContext = ctx
            });
        }
    }
}
