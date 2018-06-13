using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.Outcome.PatchOutcomeHttpTrigger
{
    public static class PatchOutcomeHttpTrigger
    {
        [FunctionName("Patch")]
        [Display(Name = "Patch", Description = "Ability to modify/update an outcome record.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Outcomes/{outcomeId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string actionplanId, string outcomeId)
        {
            log.Info("Patch Outcome C# HTTP trigger function processed a request.");

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
                Content = new StringContent("Updated Outcome record with Id of : " + outcomeGuid)
            };
        }
    }
}