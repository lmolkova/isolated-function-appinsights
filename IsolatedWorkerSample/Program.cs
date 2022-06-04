using FunctionApp5;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        // we pass configuration to AppInsights with appsettings.json
        // note: Function's host.json won't be read and it's format is
        // not compatible with vanilla ApplicationInsights Worker
        config.AddJsonFile("appsettings.json", optional: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddLogging();

        // worker and host talk over gRPC, let's filter out auto-detected calls
        services.AddApplicationInsightsTelemetryProcessor<FilteringProcessor>();

        // we just add Application Insights Worker here. It's not specific to
        // Functions, but will trace dependency calls, forward logs and everything else
        // you can expect from Application Insights SDK.
        services.AddApplicationInsightsTelemetryWorkerService();
    })
    .Build();

host.Run();