using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.Outcomes.PutOutcomesHttpTrigger
{
    public static class PutOutcomesHttpTrigger
    {
        [Disable]
        [FunctionName("Put")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Customers/{customerId}/Interactions/{interactionId}/Outcomes/{OutcomeId}")]HttpRequestMessage req, TraceWriter log, string OutcomeId)
        {
            log.Info("Put Action Plan C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(OutcomeId, out var OutcomesGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(OutcomeId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Replaced Action Plan record with Id of : " + OutcomesGuid)
            };
        }
    }
}