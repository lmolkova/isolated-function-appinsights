# Configuring Application Insights in Azure Function .NET Isolated Worker

This samples demonsrates how to manually configure Application Insights in Azure Function with .NET isolated worker. This is a temporary workaround and may become redundunt when isolated worker tracing will be fully supported.

## First steps

1. Create isolated Azure Function in .NET following [this guide](https://docs.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide)
2. Configure triggers and bindings of your choice, I used [Blob storage bindings v5](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-blob-output?tabs=isolated-process%2Cextensionv5&pivots=programming-language-csharp)
3. Add some logic to your function, e.g. DB call made from worker (not via binding) or an HTTP call.

When you run your sample with Applicatication Insights configured for the host, you won't see dependency calls you've made directly from your worker. It happens because Application Insights only monitors host process and does not cover worker process. But it's easy to fix.  

## Application Insights configuration

Azure Function host process is just another ASP.NET Core application, so we can use [Application Insights ASP.NET Core SDK](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core) or [Worker SDK](https://docs.microsoft.com/en-us/azure/azure-monitor/app/worker-service) - we'll use the latter in this sample.

1. Let's enable Application Insights - update host configuration:

```cs
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        // we pass configuration to AppInsights with appsettings.json
        // host.json is Function-specific and won't work here.
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
```

Here's [FilteringProcessor](.\IsolatedWorker\FilteringProcessor.cs) code.

Now all the dependencies that Application Insights can auto-detect will show up on the Azure Portal, but won't be correlated to Function host bindings telemetry - let's fix it.

2. Let's populate tracing context. We'll use `System.Diagnostics.Activity` and Application Insights `TelemetryClient` combination to achieve it:

```cs
[Function("Function1")]
public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, 
    FunctionContext context)
{
    // first thing after function is called, wrap it's expecution with extra request telemetry
    // and pass over host trace context explicitly.
    using var op = TrackWorkerExecution(_telemetryClient, context);
    ...
    await _httpClient.GetStringAsync("http://microsoft.com");
    return response;
}

public static IOperationHolder<RequestTelemetry> TrackWorkerExecution(
            TelemetryClient telemetryClient,
            FunctionContext context)
{
    // Activity is .NET tracing primitive and will take care of context parsing and propagation
    // we give it W3C Trace context (traceparent and tracestate) that host passed over.
    // This context prepresents host Request created to trace function invocation
    Activity workerCall = new Activity(context.FunctionDefinition.Name + "-worker");

    workerCall.SetParentId(context.TraceContext.TraceParent);
    workerCall.TraceStateString = context.TraceContext.TraceState;

    // then pass it over to Application Insights to create request telemtery.
    // It will have name we set on Activity (<Function>-worker)
    // and will be a child of Request telemetry that traces host function invocaion.
    return telemetryClient.StartOperation<RequestTelemetry>(workerCall);
}
```

Now run your sample - you should see all the dependencies on the portal and correlated to the Function host telemetry.

TODO pic.

 