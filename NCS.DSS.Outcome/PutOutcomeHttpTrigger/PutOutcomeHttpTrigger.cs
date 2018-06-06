using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.Outcome.PutOutcomeHttpTrigger
{
    public static class PutOutcomeHttpTrigger
    {
        [FunctionName("Put")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Customers/{customerId:guid}/Interactions/{interactionId:guid}/ActionPlans/{actionplanId:guid}/Outcomes/{outcomeId:guid}")]HttpRequestMessage req, TraceWriter log, string outcomeId)
        {
            log.Info("Put Outcome C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(outcomeId, out var outcomeGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(outcomeId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Replaced Outcome record with Id of : " + outcomeGuid)
            };
        }
    }
}