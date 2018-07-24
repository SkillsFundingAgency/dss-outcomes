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
using NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service;
using NCS.DSS.Outcomes.Helpers;
using NCS.DSS.Outcomes.Ioc;

namespace NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Function
{
    public static class GetOutcomesByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [ResponseType(typeof(Models.Outcomes))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Action Plan found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action Plan does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual action plan for the given customer")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/actionplans/{actionplanId}/Outcomes/{OutcomesId}")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string actionplanId, string OutcomesId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IGetOutcomesByIdHttpTriggerService outcomesGetService)
        {
            log.LogInformation("Get Outcomes By Id C# HTTP trigger function  processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(actionplanId, out var actionPlansGuid))
                return HttpResponseMessageHelper.BadRequest(actionPlansGuid);

            if (!Guid.TryParse(OutcomesId, out var outcomesGuid))
                return HttpResponseMessageHelper.BadRequest(outcomesGuid);

            //Check customer
            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            //Check interactions
                var doesInteractionExist = resourceHelper.DoesInteractionExist(interactionGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            //Check actionplans
                var doesActionPlanExist = resourceHelper.DoesActionPlanExist(actionPlansGuid);

            if (!doesActionPlanExist)
                return HttpResponseMessageHelper.NoContent(actionPlansGuid);

            var outcomes = await outcomesGetService.GetOutcomesForCustomerAsync(customerGuid, interactionGuid, actionPlansGuid, outcomesGuid);

            return outcomes == null ?
                HttpResponseMessageHelper.NoContent(outcomesGuid) :
                HttpResponseMessageHelper.Ok(outcomes);

        }
    }
}