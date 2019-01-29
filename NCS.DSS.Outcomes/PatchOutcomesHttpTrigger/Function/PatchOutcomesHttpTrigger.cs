using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.Functions.DI.Standard.Attributes;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Function
{
    public static class PatchOutcomesHttpTrigger
    {
        [FunctionName("Patch")]
        [ProducesResponseType(typeof(Models.Outcomes),200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Outcome Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Outcome does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Outcome validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update a customers Outcome record.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/sessions/{sessionId}/actionplans/{actionplanId}/Outcomes/{OutcomeId}")]HttpRequest req, ILogger log, string customerId, string interactionId, string actionplanId, string OutcomeId, string sessionId,
            [Inject]IResourceHelper resourceHelper, 
            [Inject]IPatchOutcomesHttpTriggerService outcomesPatchService,
            [Inject]IValidate validate,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IHttpRequestHelper httpRequestHelper,
            [Inject]IHttpResponseMessageHelper httpResponseMessageHelper,
            [Inject]IJsonHelper jsonHelper)
        {

            Models.Outcomes test = new Models.Outcomes
            {
                OutcomeId = Guid.Parse("4a563744-bcef-4f7e-8d89-8cdfaf3e157a"),
                CustomerId = Guid.Parse("518b8b41-ff04-4668-9bf1-62800399b90c"),
                ActionPlanId = Guid.Parse("d5529a13-fca1-4775-b456-b5ee12d02fcd"),
                SubcontractorId = "0000001212",
                OutcomeType = ReferenceData.OutcomeType.CareersManagement,
                OutcomeClaimedDate = DateTime.Parse("01/05/2018"),
                OutcomeEffectiveDate = DateTime.Parse("04/04/2018"),
                ClaimedPriorityGroupId = ReferenceData.ClaimedPriorityGroupId.AdultsWithSpecialEducationalNeedsAndOrDisabilities,
                TouchpointId = "0000000010",
                LastModifiedDate = DateTime.Parse("05/01/2019"),
                LastModifiedTouchpointId = "000000010"
            };

            return httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectAndRenameIdProperty(test, "id", "OutcomeId"));


            //var touchpointId = httpRequestHelper.GetDssTouchpointId(req);
            //if (string.IsNullOrEmpty(touchpointId))
            //{
            //    log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
            //    return httpResponseMessageHelper.BadRequest();
            //}

            //var ApimURL = httpRequestHelper.GetDssApimUrl(req);
            //if (string.IsNullOrEmpty(ApimURL))
            //{
            //    log.LogInformation("Unable to locate 'apimurl' in request header");
            //    return httpResponseMessageHelper.BadRequest();
            //}

            //log.LogInformation("Patch Action Plan C# HTTP trigger function processed a request. " + touchpointId);

            //if (!Guid.TryParse(customerId, out var customerGuid))
            //    return httpResponseMessageHelper.BadRequest(customerGuid);

            //if (!Guid.TryParse(interactionId, out var interactionGuid))
            //    return httpResponseMessageHelper.BadRequest(interactionGuid);

            //if (!Guid.TryParse(actionplanId, out var actionplanGuid))
            //    return httpResponseMessageHelper.BadRequest(actionplanGuid);

            //if (!Guid.TryParse(OutcomeId, out var outcomesGuid))
            //    return httpResponseMessageHelper.BadRequest(outcomesGuid);

            //Models.OutcomesPatch  outcomesPatchRequest;

            //try
            //{
            //    outcomesPatchRequest = await httpRequestHelper.GetResourceFromRequest<Models.OutcomesPatch>(req);
            //}
            //catch (JsonException ex)
            //{
            //    return httpResponseMessageHelper.UnprocessableEntity(ex);
            //}

            //if (outcomesPatchRequest == null)
            //    return httpResponseMessageHelper.UnprocessableEntity(req);

            //outcomesPatchRequest.LastModifiedTouchpointId = touchpointId;

            //var errors = validate.ValidateResource(outcomesPatchRequest);

            //if (errors != null && errors.Any())
            //    return httpResponseMessageHelper.UnprocessableEntity(errors);

            //var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            //if (!doesCustomerExist)
            //    return httpResponseMessageHelper.NoContent(customerGuid);

            //var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            //if (isCustomerReadOnly)
            //    return httpResponseMessageHelper.Forbidden(customerGuid);

            //var doesInteractionExist = resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            //if (!doesInteractionExist)
            //    return httpResponseMessageHelper.NoContent(interactionGuid);

            //var doesActionPlanExist = resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionplanGuid, interactionGuid, customerGuid);

            //if (!doesActionPlanExist)
            //    return httpResponseMessageHelper.NoContent(actionplanGuid);

            //var outcomes = await outcomesPatchService.GetOutcomesForCustomerAsync(customerGuid, interactionGuid, actionplanGuid, outcomesGuid);

            //if (outcomes == null)
            //    return httpResponseMessageHelper.NoContent(outcomesGuid);

            //var updatedOutcomes = await outcomesPatchService.UpdateAsync(outcomes, outcomesPatchRequest);

            //if (updatedOutcomes != null)
            //    await outcomesPatchService.SendToServiceBusQueueAsync(updatedOutcomes,customerGuid, ApimURL);

            //return updatedOutcomes == null ?
            //    httpResponseMessageHelper.BadRequest(outcomesGuid) :
            //    httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectAndRenameIdProperty(outcomes, "id", "OutcomeId"));

        }
    }
}
