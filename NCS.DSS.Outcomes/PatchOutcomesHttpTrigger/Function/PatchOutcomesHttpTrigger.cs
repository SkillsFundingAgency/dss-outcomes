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
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/Sessions/{sessionId}/ActionPlans/{actionplanId}/Outcomes/{outcomeId}")]HttpRequest req, ILogger log, string customerId, string interactionId, string sessionId, string actionplanId, string outcomeId, 
            [Inject]IResourceHelper resourceHelper, 
            [Inject]IPatchOutcomesHttpTriggerService outcomesPatchService,
            [Inject]IValidate validate,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IHttpRequestHelper httpRequestHelper,
            [Inject]IHttpResponseMessageHelper httpResponseMessageHelper,
            [Inject]IJsonHelper jsonHelper)
        {
            loggerHelper.LogMethodEnter(log);

            var correlationId = httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
                return httpResponseMessageHelper.BadRequest();
            }

            var ApimURL = httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return httpResponseMessageHelper.BadRequest();
            }

            var subcontractorId = httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'SubcontractorId' in request header");

            loggerHelper.LogInformationMessage(log, correlationGuid,
                string.Format("Patch Outcome C# HTTP trigger function  processed a request. By Touchpoint: {0}",
                    touchpointId));

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'customerId' to a Guid: {0}", customerId));
                return httpResponseMessageHelper.BadRequest(customerGuid);
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'interactionId' to a Guid: {0}", interactionId));
                return httpResponseMessageHelper.BadRequest(interactionGuid);
            }

            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'sessionId' to a Guid: {0}", sessionGuid));
                return httpResponseMessageHelper.BadRequest(sessionGuid);
            }

            if (!Guid.TryParse(actionplanId, out var actionPlanGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'actionplanId' to a Guid: {0}", actionplanId));
                return httpResponseMessageHelper.BadRequest(actionPlanGuid);
            }

            if (!Guid.TryParse(outcomeId, out var outcomesGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'outcomeId' to a Guid: {0}", outcomeId));
                return httpResponseMessageHelper.BadRequest(outcomesGuid);
            }

            Models.OutcomesPatch outcomesPatchRequest;

            try
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to get resource from body of the request");
                outcomesPatchRequest = await httpRequestHelper.GetResourceFromRequest<Models.OutcomesPatch>(req);
            }
            catch (JsonException ex)
            {
                loggerHelper.LogError(log, correlationGuid, "Unable to retrieve body from req", ex);
                return httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (outcomesPatchRequest == null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "outcome patch request is null");
                return httpResponseMessageHelper.UnprocessableEntity(req);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to set id's for action plan patch");
            outcomesPatchRequest.SetIds(touchpointId, subcontractorId);

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer does not exist {0}", customerGuid));
                return httpResponseMessageHelper.NoContent(customerGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if this is a read only customer {0}", customerGuid));
            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer is read only {0}", customerGuid));
                return httpResponseMessageHelper.Forbidden(customerGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Session {0} for customer {1}", sessionGuid, customerGuid));
            var doesSessionExist = resourceHelper.DoesSessionExistAndBelongToCustomer(sessionGuid, interactionGuid, customerGuid);

            if (!doesSessionExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Session does not exist {0}", sessionGuid));
                return httpResponseMessageHelper.NoContent(sessionGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get GetDateAndTimeOfSession for Session {0}", sessionGuid));
            var dateAndTimeOfSession = await resourceHelper.GetDateAndTimeOfSession(sessionGuid);

            if (!dateAndTimeOfSession.HasValue)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to get GetDateAndTimeOfSession{0}", sessionGuid));
                return httpResponseMessageHelper.NoContent(sessionGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get action plan {0} for customer {1}", actionPlanGuid, customerGuid));
            var doesActionPlanExist = resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("ActionPlan does not exist {0}", actionPlanGuid));
                return httpResponseMessageHelper.NoContent(actionPlanGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Outcome {0} for customer {1}", outcomesGuid, customerGuid));
            var outcomes = await outcomesPatchService.GetOutcomesForCustomerAsync(customerGuid, interactionGuid, actionPlanGuid, outcomesGuid);

            if (outcomes == null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Outcome does not exist {0}", outcomesGuid));
                return httpResponseMessageHelper.NoContent(outcomesGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = validate.ValidateResource(outcomesPatchRequest, dateAndTimeOfSession.Value);

            if (errors != null && errors.Any())
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "validation errors with resource");
                return httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to update Outcome {0}", outcomesGuid));
            var updatedOutcomes = await outcomesPatchService.UpdateAsync(outcomes, outcomesPatchRequest, outcomesGuid);

            if (updatedOutcomes != null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("attempting to send to service bus {0}", outcomesGuid));
                await outcomesPatchService.SendToServiceBusQueueAsync(updatedOutcomes, customerGuid, ApimURL);
            }

            loggerHelper.LogMethodExit(log);

            return updatedOutcomes == null ?
                httpResponseMessageHelper.BadRequest(outcomesGuid) :
                httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectAndRenameIdProperty(outcomes, "id", "OutcomeId"));

        }
    }
}
