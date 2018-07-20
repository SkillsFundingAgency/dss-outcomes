using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.Outcomes.DeleteOutcomesHttpTrigger
{
    public static class DeleteOutcomesHttpTrigger
    {
        [Disable]
        [FunctionName("Delete")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Customers/{customerId}/Interactions/{interactionId}/Outcomes/{OutcomesId}")]HttpRequestMessage req, TraceWriter log, string OutcomesId)
        {
            log.Info("Delete Action Plan C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(OutcomesId, out var OutcomesGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(OutcomesId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Deleted Action Plan record with Id of : " + OutcomesGuid)
            };
        }
    }
}