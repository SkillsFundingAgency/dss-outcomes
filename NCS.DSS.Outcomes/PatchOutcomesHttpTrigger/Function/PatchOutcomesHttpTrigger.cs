using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using JsonException = Newtonsoft.Json.JsonException;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Function
{
    public class PatchOutcomesHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IPatchOutcomesHttpTriggerService _outcomesPatchService;
        private readonly IJsonHelper _jsonHelper;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IValidate _validate;
        private readonly ILogger log;
        public PatchOutcomesHttpTrigger(IResourceHelper resourceHelper,
            IHttpRequestHelper httpRequestHelper,
            IPatchOutcomesHttpTriggerService outcomesPatchService,
            IJsonHelper jsonHelper,
            ILoggerHelper loggerHelper,
            IValidate validate,
            ILogger<PatchOutcomesHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestHelper = httpRequestHelper;
            _outcomesPatchService = outcomesPatchService;
            _jsonHelper = jsonHelper;
            _loggerHelper = loggerHelper;
            _validate = validate;
            log = logger;
        }
        [Function("Patch")]
        [ProducesResponseType(typeof(Models.Outcomes), 200)]
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
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Outcomes/{outcomeId}")] HttpRequest req, string customerId, string interactionId, string actionplanId, string outcomeId)
        {
            _loggerHelper.LogMethodEnter(log);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
                return new BadRequestObjectResult("Unable to locate 'APIM-TouchpointId' in request header.");
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'SubcontractorId' in request header");
                return new BadRequestObjectResult("Unable to locate 'SubcontractorId' in request header");
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return new BadRequestObjectResult("Unable to locate 'apimurl' in request header");
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid,
                string.Format("Patch Outcome C# HTTP trigger function  processed a request. By Touchpoint: {0}",
                    touchpointId));

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'customerId' to a Guid: {0}", customerId));
                return new BadRequestObjectResult(customerGuid);
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'interactionId' to a Guid: {0}", interactionId));
                return new BadRequestObjectResult(interactionGuid);
            }

            if (!Guid.TryParse(actionplanId, out var actionPlanGuid))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'actionplanId' to a Guid: {0}", actionplanId));
                return new BadRequestObjectResult(actionPlanGuid);
            }

            if (!Guid.TryParse(outcomeId, out var outcomesGuid))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'outcomeId' to a Guid: {0}", outcomeId));
                return new BadRequestObjectResult(outcomesGuid);
            }

            Models.OutcomesPatch outcomesPatchRequest;

            var setOutcomeClaimedDateToNull = false;
            var setOutcomeEffectiveDateToNull = false;
            int requestCount = 0;
            string requestBody;
            req.EnableBuffering(); //Allows request to be read multiple times

            try
            {
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                req.Body.Position = 0;
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(log, correlationGuid, "Unable to read request Body", ex);
                throw;
            }

            if (!string.IsNullOrEmpty(requestBody))
            {
                var outcomeClaimedDate = _jsonHelper.GetValue(requestBody, "OutcomeClaimedDate");

                if (outcomeClaimedDate == string.Empty)
                    setOutcomeClaimedDateToNull = true;

                var outcomeEffectiveDate = _jsonHelper.GetValue(requestBody, "OutcomeEffectiveDate");

                if (outcomeEffectiveDate == string.Empty)
                    setOutcomeEffectiveDateToNull = true;

                var requestObject = JObject.Parse(requestBody);

                if (requestObject != null)
                    requestCount = requestObject.Count;
            }

            try
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to get resource from body of the request");
                outcomesPatchRequest = await _httpRequestHelper.GetResourceFromRequest<Models.OutcomesPatch>(req);
            }
            catch (JsonException ex)
            {
                _loggerHelper.LogError(log, correlationGuid, "Unable to retrieve body from req", ex);
                return new UnprocessableEntityObjectResult(ex);
            }

            if (outcomesPatchRequest == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "outcome patch request is null");
                return new UnprocessableEntityObjectResult(req);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to set id's for action plan patch");
            outcomesPatchRequest.SetIds(touchpointId, subcontractorId);

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer does not exist {0}", customerGuid));
                return new NoContentResult();
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if this is a read only customer {0}", customerGuid));
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            var isADuplicateCustomer = _resourceHelper.GetCustomerReasonForTermination();

            if (isCustomerReadOnly && isADuplicateCustomer != 3)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer is read only {0}", customerGuid));
                return new ForbidResult(customerGuid.ToString());
            }

            if (isADuplicateCustomer == 3)
            {
                if (requestCount > 2 ||
                    requestCount == 1 && !setOutcomeClaimedDateToNull &&
                    requestCount == 1 && !setOutcomeEffectiveDateToNull ||
                    (requestCount == 2 && (!setOutcomeClaimedDateToNull ||
                                           !setOutcomeEffectiveDateToNull)))
                {
                    return new ForbidResult(new HttpErrorResponse(new List<string>
                    {
                        "Duplicate Customer: This resource is read only. You may only remove values for Outcome Claimed and Effective date"
                    }, correlationGuid).ToString());

                }
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Interaction {0} for customer {1}", interactionGuid, customerGuid));
            var doesInteractionExist = _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Interaction does not exist {0}", interactionGuid));
                return new NoContentResult();
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get action plan {0} for customer {1}", actionPlanGuid, customerGuid));
            var doesActionPlanExist = _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("ActionPlan does not exist {0}", actionPlanGuid));
                return new NoContentResult();
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Outcome {0} for customer {1}", outcomesGuid, customerGuid));
            var outcome = await _outcomesPatchService.GetOutcomesForCustomerAsync(customerGuid, interactionGuid, actionPlanGuid, outcomesGuid);

            if (outcome == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Outcome does not exist {0}", outcomesGuid));
                return new NoContentResult();
            }

            var patchedOutcomeResource = _outcomesPatchService.PatchResource(outcome, outcomesPatchRequest);

            if (patchedOutcomeResource == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Outcome does not exist {0}", actionPlanGuid));
                return new NoContentResult();
            }

            if (setOutcomeClaimedDateToNull || setOutcomeEffectiveDateToNull)
            {
                patchedOutcomeResource =
                    _outcomesPatchService.UpdateOutcomeClaimedDateOutcomeEffectiveDateValue(patchedOutcomeResource,
                        setOutcomeClaimedDateToNull, setOutcomeEffectiveDateToNull);
            }

            Models.Outcomes outcomeValidationObject;

            try
            {
                outcomeValidationObject = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomeResource);
            }
            catch (JsonException ex)
            {
                _loggerHelper.LogError(log, correlationGuid, "Unable to Deserialize Object", ex);
                throw;
            }

            if (outcomeValidationObject == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Action Plan Validation Object is null");
                return new UnprocessableEntityObjectResult(req);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get GetDateAndTimeOfSession for Session {0}", outcomeValidationObject.SessionId));
            var dateAndTimeOfSession = await _resourceHelper.GetDateAndTimeOfSession(outcomeValidationObject.SessionId.GetValueOrDefault());

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = _validate.ValidateResource(outcomeValidationObject, dateAndTimeOfSession);

            if (errors != null && errors.Any())
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "validation errors with resource");
                return new UnprocessableEntityObjectResult(errors);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to update Outcome {0}", outcomesGuid));
            var updatedOutcome = await _outcomesPatchService.UpdateCosmosAsync(patchedOutcomeResource, outcomesGuid);

            if (updatedOutcome != null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("attempting to send to service bus {0}", outcomesGuid));
                await _outcomesPatchService.SendToServiceBusQueueAsync(updatedOutcome, customerGuid, apimUrl);
            }

            _loggerHelper.LogMethodExit(log);

            return updatedOutcome == null
                ? new BadRequestObjectResult(outcomesGuid)
                : new JsonResult(updatedOutcome, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
        }
    }
}
