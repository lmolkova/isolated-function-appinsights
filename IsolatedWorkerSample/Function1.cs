using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApp5
{
    public class Function1
    {
        private readonly ILogger _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly HttpClient _httpClient;

        public Function1(ILoggerFactory loggerFactory, TelemetryClient telemteryClient)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
            _telemetryClient = telemteryClient;
            _httpClient = new HttpClient();
        }

        [Function("Function1")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, 
            FunctionContext context)
        {
            // first thing after function is called, wrap it's expecution with extra request telemetry
            // and pass over host trace context explicitly.
            using var op = TelemetryUtils.trackWorkerExecution(_telemetryClient, context);

            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            await _httpClient.GetStringAsync("http://microsoft.com");
            return response;
        }
    }
}
