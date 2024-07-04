using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Serilog;

namespace ChatSuite.Sdk.Extensions.DependencyInjection;

public static class LoggingExtensions
{
    public static IFunctionsHostBuilder AddLogger(this IFunctionsHostBuilder builder, IConfiguration configuration)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        builder.Services.AddLogging(builder => builder.AddSerilog(loggerConfiguration));
        return builder;
    }

    public static IHostBuilder AddLogger(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureLogging(loggingBuilder => loggingBuilder.Services.AddSingleton<ILoggerProvider, DefaultLoggerProvider>());
        hostBuilder.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));
        return hostBuilder;
    }

    public static IHostBuilder UseDefaultLogger(this IHostBuilder builder) => builder
        .ConfigureLogging(loggingBuilder => loggingBuilder.Services.AddSingleton<ILoggerProvider, DefaultLoggerProvider>())
        .UseSerilog((context, services, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(context.Configuration));
}

