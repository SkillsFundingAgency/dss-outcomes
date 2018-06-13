using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace NCS.DSS.Outcome.PostOutcomeHttpTrigger
{
    public static class PostOutcomeHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Outcome))]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId:guid}/Interactions/{interactionId:guid}/ActionPlans/{actionplanId:guid}/Outcomes/")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Post Outcome C# HTTP trigger function processed a request.");

            // Get request body
            var outcome = await req.Content.ReadAsAsync<Models.Outcome>();

            var outcomeService = new PostOutcomeHttpTriggerService();
            var outcomeId = outcomeService.Create(outcome);

            return outcomeId == null
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                : new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("Created Outcome record with Id of : " + outcomeId)
                };
        }
    }
}