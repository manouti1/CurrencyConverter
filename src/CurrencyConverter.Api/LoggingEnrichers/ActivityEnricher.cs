namespace CurrencyConverter.Api.LoggingEnrichers
{
    using Serilog.Core;
    using Serilog.Events;
    using System.Diagnostics;

    public class ActivityEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory pf)
        {
            var traceId = Activity.Current?.TraceId.ToString();
            if (!string.IsNullOrEmpty(traceId))
            {
                logEvent.AddPropertyIfAbsent(pf.CreateProperty("TraceId", traceId));
            }
        }
    }
}
