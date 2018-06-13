using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Outcome.GetOutcomeByIdHttpTrigger
{
    public static class GetOutcomeByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [Display(Name = "Get", Description = "Ability to retrieve an individual outcome record.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Outcomes/{outcomeId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string actionplanId, string outcomeId)
        {
            log.Info("Get Outcome By Id C# HTTP trigger function  processed a request.");

            if (!Guid.TryParse(outcomeId, out var outcomeGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(outcomeId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            var service = new GetOutcomeByIdHttpTriggerService();
            var values = await service.GetOutcome(outcomeGuid);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}