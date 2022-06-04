using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp5
{
    internal class FilteringProcessor : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        // next will point to the next TelemetryProcessor in the chain.
        public FilteringProcessor(ITelemetryProcessor next)
        {
            this.Next = next;
        }

        public void Process(ITelemetry item)
        {
            if (item is DependencyTelemetry dependency &&
                dependency.Type == "Http" &&
                dependency.Target.StartsWith("127.0.0.1"))
            {
                return;
            }

            this.Next.Process(item);
        }
    }
}
