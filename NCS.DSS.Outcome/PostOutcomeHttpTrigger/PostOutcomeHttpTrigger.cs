using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Outcome.Annotations;

namespace NCS.DSS.Outcome.PostOutcomeHttpTrigger
{
    public static class PostOutcomeHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Outcome))]
        [OutcomeResponse(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Outcome created", ShowSchema = true)]
        [OutcomeResponse(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Unable to create Outcome", ShowSchema = false)]
        [OutcomeResponse(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Forbidden", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new outcome resource.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Outcomes/")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string actionplanId)
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