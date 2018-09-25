using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Annotations;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.Helpers;
using NCS.DSS.Outcomes.Ioc;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Function
{
    public static class PatchOutcomesHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.Outcomes))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Action Plan Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action Plan does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Action Plan validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update a customers action plan record.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/actionplans/{actionplanId}/Outcomes/{OutcomeId}")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string actionplanId, string OutcomeId,
            [Inject]IResourceHelper resourceHelper, 
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPatchOutcomesHttpTriggerService outcomesPatchService)
        {
            var touchpointId = httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
                return HttpResponseMessageHelper.BadRequest();
            }

            var ApimURL = httpRequestMessageHelper.GetApimURL(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Patch Action Plan C# HTTP trigger function processed a request. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(actionplanId, out var actionplanGuid))
                return HttpResponseMessageHelper.BadRequest(actionplanGuid);

            if (!Guid.TryParse(OutcomeId, out var outcomesGuid))
                return HttpResponseMessageHelper.BadRequest(outcomesGuid);

            Models.OutcomesPatch  outcomesPatchRequest;

            try
            {
                outcomesPatchRequest = await httpRequestMessageHelper.GetOutcomesFromRequest<Models.OutcomesPatch>(req);
            }
            catch (JsonException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (outcomesPatchRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            outcomesPatchRequest.LastModifiedTouchpointId = touchpointId;

            var errors = validate.ValidateResource(outcomesPatchRequest);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return HttpResponseMessageHelper.Forbidden(customerGuid);

            var doesInteractionExist = resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var doesActionPlanExist = resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionplanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
                return HttpResponseMessageHelper.NoContent(actionplanGuid);

            var outcomes = await outcomesPatchService.GetOutcomesForCustomerAsync(customerGuid, interactionGuid, actionplanGuid, outcomesGuid);

            if (outcomes == null)
                return HttpResponseMessageHelper.NoContent(outcomesGuid);

            var updatedOutcomes = await outcomesPatchService.UpdateAsync(outcomes, outcomesPatchRequest);

            if (updatedOutcomes != null)
                await outcomesPatchService.SendToServiceBusQueueAsync(updatedOutcomes,customerGuid, ApimURL);

            return updatedOutcomes == null ?
                HttpResponseMessageHelper.BadRequest(outcomesGuid) :
                HttpResponseMessageHelper.Ok(JsonHelper.SerializeObject(updatedOutcomes));

        }
    }
}
