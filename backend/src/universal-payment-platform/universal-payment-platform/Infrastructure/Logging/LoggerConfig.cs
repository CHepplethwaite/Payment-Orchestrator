using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

namespace UniversalPaymentPlatform.Infrastructure.Logging
{
    public static class LoggerConfig
    {
        /// <summary>
        /// Configures Serilog for production-grade structured logging.
        /// Call this early in Program.cs before building the host.
        /// </summary>
        public static void ConfigureSerilog(IConfiguration configuration)
        {
            // Determine environment
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            // Create logger configuration
            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Environment", environment)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .WriteTo.Console(
                    theme: AnsiConsoleTheme.Code,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
                )
                .WriteTo.File(
                    path: "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    shared: true,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
                );

            // Optional: Compact JSON sink for structured ingestion into ELK, Seq, or Grafana Loki
            loggerConfiguration.WriteTo.File(
                new CompactJsonFormatter(),
                path: "Logs/structured-log-.json",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10,
                shared: true
            );

            // Optional: Add Seq or Elasticsearch sink
            var seqUrl = configuration["Logging:Seq:Url"];
            if (!string.IsNullOrWhiteSpace(seqUrl))
            {
                loggerConfiguration.WriteTo.Seq(seqUrl);
            }

            Log.Logger = loggerConfiguration.CreateLogger();

            Log.Information("Logger initialized for {Environment} environment", environment);
        }
    }
}
