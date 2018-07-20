using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Annotations;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Helpers;
using NCS.DSS.Outcomes.Ioc;

namespace NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Function
{
    public static class GetOutcomesHttpTrigger
    {
        [FunctionName("Get")]
        [ResponseType(typeof(Models.Outcomes))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Action Plans found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action Plans do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return all action plans for the given Outcomes.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/actionplans/{actionplanId}/Outcomes")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string actionplanId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IGetOutcomesHttpTriggerService OutcomesGetService)
        {
            log.LogInformation("Get Outcomes C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var Outcomes = await OutcomesGetService.GetOutcomesAsync(customerGuid);

            return Outcomes == null ?
                HttpResponseMessageHelper.NoContent(customerGuid) :
                HttpResponseMessageHelper.Ok(Outcomes);

        }
    }
}