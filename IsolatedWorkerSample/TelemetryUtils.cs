using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using System.Diagnostics;

namespace FunctionApp5
{
    internal class TelemetryUtils
    {
        public static IOperationHolder<RequestTelemetry> trackWorkerExecution(
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
    }
}
