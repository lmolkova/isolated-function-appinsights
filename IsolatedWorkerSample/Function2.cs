
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionApp5
{
    public class Function2
    {
        private readonly ILogger _logger;
        private readonly TelemetryClient _telemetryClient;

        public Function2(ILoggerFactory loggerFactory, TelemetryClient telemetryClient)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
            _telemetryClient = telemetryClient;
        }


        [Function("BlobTrigger")]
        [BlobOutput("uploads/{name}-output.txt")]
        public string Run([BlobTrigger("newtest/{name}")] string myBlob, string name, FunctionContext context)
        {
            using var op = TelemetryUtils.trackWorkerExecution(_telemetryClient, context);
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {myBlob}");

            return "blob-output content";
        }
    }
}
