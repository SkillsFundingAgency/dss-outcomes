using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Functions.DI.Standard.Attributes;
using DFC.Swagger.Standard.Annotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.DeleteOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Helpers;

namespace NCS.DSS.Outcomes.DeleteOutcomesHttpTrigger.Function
{
    public static class DeleteOutcomesHttpTrigger
    {
        [Disable]
        [FunctionName("Delete")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Outcome deleted", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Outcome does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Delete", Description = "Ability to remove a customers Outcome record.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Customers/{customerId}/Interactions/{interactionId}/actionplans/{actionplanId}/Outcomes/{outcomeId}")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string actionplanId, string outcomeId,
        [Inject]IResourceHelper resourceHelper,
        [Inject]IDeleteOutcomesHttpTriggerService outcomesDeleteService)
        {
            log.LogInformation("Delete Action Plan C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(actionplanId, out var actionplanGuid))
                return HttpResponseMessageHelper.BadRequest(actionplanGuid);

            if (!Guid.TryParse(outcomeId, out var outcomesGuid))
                return HttpResponseMessageHelper.BadRequest(outcomesGuid);

            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var doesInteractionExist = resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var doesActionPlanExist = resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionplanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
                return HttpResponseMessageHelper.NoContent(actionplanGuid);

            var outcome = await outcomesDeleteService.GetOutcomeForCustomerAsync(customerGuid, interactionGuid, actionplanGuid, outcomesGuid);

            if (outcome == null)
                return HttpResponseMessageHelper.NoContent(outcomesGuid);

            var outcomeDeleted = await outcomesDeleteService.DeleteAsync(outcome.OutcomeId.GetValueOrDefault());

            return !outcomeDeleted ?
                HttpResponseMessageHelper.BadRequest(outcomesGuid) :
                HttpResponseMessageHelper.Ok();

        }
    }
}