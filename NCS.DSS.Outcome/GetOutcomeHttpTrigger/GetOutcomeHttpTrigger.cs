using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;

namespace NCS.DSS.Outcome.GetOutcomeHttpTrigger
{
    public static class GetOutcomeHttpTrigger
    {
        [FunctionName("Get")]
        [ResponseType(typeof(Models.Outcome))]
        [Display(Name = "Get", Description = "Ability to return all outcome records for an individual customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Outcomes/")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string actionplanId)
        {
            log.Info("Get Outcomes C# HTTP trigger function processed a request.");

            var service = new GetOutcomeHttpTriggerService();
            var values = await service.GetOutcomes();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}