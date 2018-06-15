using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Outcome.Annotations;
using Newtonsoft.Json;

namespace NCS.DSS.Outcome.DeleteOutcomeHttpTrigger
{
    public static class DeleteOutcomeHttpTrigger
    {
        [FunctionName("Delete")]
        [OutcomeResponse(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Outcome deleted", ShowSchema = true)]
        [OutcomeResponse(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Supplied Outcome Id does not exist", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to delete a outcome record.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Outcomes/{outcomeId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string actionplanId, string outcomeId)
        {
            log.Info("Delete Outcome C# HTTP trigger function processed a request.");

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
                Content = new StringContent("Deleted Outcome record with Id of : " + outcomeGuid)
            };
        }
    }
}