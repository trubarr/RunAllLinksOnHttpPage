using Serilog;
using Serilog.Configuration;

namespace RunAllLinksOnHttpPage.Logging
{
    static class LoggerCallerEnrichmentConfiguration
    {
        public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With<CallerEnricher>();
        }
    }
}