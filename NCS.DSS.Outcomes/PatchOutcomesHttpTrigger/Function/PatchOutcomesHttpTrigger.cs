using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
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
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Functions.Worker;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Function
{
    public static class PatchOutcomesHttpTrigger
    {
        [Function("Patch")]
        [ProducesResponseType(typeof(Models.Outcomes),200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Outcome Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Outcome does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Outcome validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update a customers Outcome record. <br>" +
                                               "<br> <b>Validation Rules:</b> <br>" +
                                               "<br><b>OutcomeClaimedDate:</b> OutcomeClaimedDate >= OutcomeEffectiveDate <br>" +
                                               "<br><b>OutcomeEffectiveDate:</b> <br>" +
                                               "When OutcomeType of: <br>" +
                                               "<ul><li>Customer Satisfaction</li> <br>" +
                                               "<li>Career Management, </li> <br>" +
                                               "<li>Accredited Learning, </li> <br>" +
                                               "<li>Career Progression </li></ul> <br>" +
                                               "Rule = OutcomeEffectiveDate >= Session.DateAndTimeOfSession AND <= Session.DateAndTimeOfSession + 12 months <br>" +
                                               "<br> When OutcomeType of: <br>" +
                                               "<br><ul><li>Sustainable Employment </li> </ul><br>" +
                                               "Rule = OutcomeEffectiveDate >= Session.DateAndTimeOfSession AND <= Session.DateAndTimeOfSession + 13 months <br>" +
                                               "<br><b>ClaimedPriorityGroup:</b> This is mandatory if OutcomeClaimedDate has a value")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Outcomes/{outcomeId}")]HttpRequest req, ILogger log, string customerId, string interactionId, string actionplanId, string outcomeId, 
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

            var subcontractorId = httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'SubcontractorId' in request header");
                return httpResponseMessageHelper.BadRequest();
            }

            var apimUrl = httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return httpResponseMessageHelper.BadRequest();
            }

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
            
            var setOutcomeClaimedDateToNull = false;
            var setOutcomeEffectiveDateToNull = false;
            int requestCount = 0;
            string requestBody;

            try
            {
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                req.Body.Position = 0;
            }
            catch (Exception ex)
            {
                loggerHelper.LogException(log, correlationGuid, "Unable to read request Body", ex);
                throw;
            }

            if (!string.IsNullOrEmpty(requestBody))
            {
                var outcomeClaimedDate = jsonHelper.GetValue(requestBody, "OutcomeClaimedDate");

                if (outcomeClaimedDate == string.Empty)
                    setOutcomeClaimedDateToNull = true;

                var outcomeEffectiveDate = jsonHelper.GetValue(requestBody, "OutcomeEffectiveDate");

                if (outcomeEffectiveDate == string.Empty)
                    setOutcomeEffectiveDateToNull = true;

                var requestObject = JObject.Parse(requestBody);

                if (requestObject != null)
                    requestCount = requestObject.Count;
            }

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
            var isCustomerReadOnly = resourceHelper.IsCustomerReadOnly();

            var isADuplicateCustomer = resourceHelper.GetCustomerReasonForTermination();

            if (isCustomerReadOnly && isADuplicateCustomer != 3)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer is read only {0}", customerGuid));
                return httpResponseMessageHelper.Forbidden(customerGuid);
            }

            if (isADuplicateCustomer == 3)
            {
                if (requestCount > 2 ||
                    requestCount == 1 && !setOutcomeClaimedDateToNull &&
                    requestCount == 1 && !setOutcomeEffectiveDateToNull ||
                    (requestCount == 2 && (!setOutcomeClaimedDateToNull ||
                                           !setOutcomeEffectiveDateToNull)))
                {
                    return httpResponseMessageHelper.Forbidden(new HttpErrorResponse(new List<string>
                    {
                        "Duplicate Customer: This resource is read only. You may only remove values for Outcome Claimed and Effective date"
                    }, correlationGuid));

                }
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Interaction {0} for customer {1}", interactionGuid, customerGuid));
            var doesInteractionExist = resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Interaction does not exist {0}", interactionGuid));
                return httpResponseMessageHelper.NoContent(interactionGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get action plan {0} for customer {1}", actionPlanGuid, customerGuid));
            var doesActionPlanExist = resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("ActionPlan does not exist {0}", actionPlanGuid));
                return httpResponseMessageHelper.NoContent(actionPlanGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Outcome {0} for customer {1}", outcomesGuid, customerGuid));
            var outcome = await outcomesPatchService.GetOutcomesForCustomerAsync(customerGuid, interactionGuid, actionPlanGuid, outcomesGuid);

            if (outcome == null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Outcome does not exist {0}", outcomesGuid));
                return httpResponseMessageHelper.NoContent(outcomesGuid);
            }
            
            var patchedOutcomeResource = outcomesPatchService.PatchResource(outcome, outcomesPatchRequest);

            if (patchedOutcomeResource == null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Outcome does not exist {0}", actionPlanGuid));
                return httpResponseMessageHelper.NoContent(actionPlanGuid);
            }

            if (setOutcomeClaimedDateToNull || setOutcomeEffectiveDateToNull)
            {
                patchedOutcomeResource =
                    outcomesPatchService.UpdateOutcomeClaimedDateOutcomeEffectiveDateValue(patchedOutcomeResource,
                        setOutcomeClaimedDateToNull, setOutcomeEffectiveDateToNull);
            }

            Models.Outcomes outcomeValidationObject;

            try
            {
                outcomeValidationObject = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomeResource);
            }
            catch (JsonException ex)
            {
                loggerHelper.LogError(log, correlationGuid, "Unable to Deserialize Object", ex);
                throw;
            }

            if (outcomeValidationObject == null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Action Plan Validation Object is null");
                return httpResponseMessageHelper.UnprocessableEntity(req);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get GetDateAndTimeOfSession for Session {0}", outcomeValidationObject.SessionId));
            var dateAndTimeOfSession = await resourceHelper.GetDateAndTimeOfSession(outcomeValidationObject.SessionId.GetValueOrDefault());

            loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = validate.ValidateResource(outcomeValidationObject, dateAndTimeOfSession);

            if (errors != null && errors.Any())
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "validation errors with resource");
                return httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to update Outcome {0}", outcomesGuid));
            var updatedOutcome = await outcomesPatchService.UpdateCosmosAsync(patchedOutcomeResource, outcomesGuid);

            if (updatedOutcome != null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("attempting to send to service bus {0}", outcomesGuid));
                await outcomesPatchService.SendToServiceBusQueueAsync(updatedOutcome, customerGuid, apimUrl);
            }

            loggerHelper.LogMethodExit(log);

            return updatedOutcome == null ?
                httpResponseMessageHelper.BadRequest(outcomesGuid) :
                httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectAndRenameIdProperty(updatedOutcome, "id", "OutcomeId"));

        }

    }
}
