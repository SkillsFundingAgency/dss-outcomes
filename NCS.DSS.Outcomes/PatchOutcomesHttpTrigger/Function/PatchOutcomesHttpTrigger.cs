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
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Function
{
    public class PatchOutcomesHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IPatchOutcomesHttpTriggerService _outcomesPatchService;
        private readonly IJsonHelper _jsonHelper;
        private readonly IValidate _validate;
        private readonly ILogger<PatchOutcomesHttpTrigger> _logger;
        private readonly IDynamicHelper _dynamicHelper;
        private static readonly string[] ExceptionToExclude = { "TargetSite" };

        public PatchOutcomesHttpTrigger(IResourceHelper resourceHelper,
            IHttpRequestHelper httpRequestHelper,
            IPatchOutcomesHttpTriggerService outcomesPatchService,
            IJsonHelper jsonHelper,
            IValidate validate,
            ILogger<PatchOutcomesHttpTrigger> logger,
            IDynamicHelper dynamicHelper)
        {
            _resourceHelper = resourceHelper;
            _httpRequestHelper = httpRequestHelper;
            _outcomesPatchService = outcomesPatchService;
            _jsonHelper = jsonHelper;
            _validate = validate;
            _logger = logger;
            _dynamicHelper = dynamicHelper;
        }

        [Function("Patch")]
        [ProducesResponseType(typeof(Models.Outcomes), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Outcome Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Outcome does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Outcome validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = 
            @"Ability to modify/update a customers Outcome record. <br> <br> <b>Validation Rules:</b> <br> 
              <br><b>OutcomeClaimedDate:</b> OutcomeClaimedDate >= OutcomeEffectiveDate <br> <br> <b>OutcomeEffectiveDate:</b> 
              <br> When OutcomeType of: <br> <ul><li>Customer Satisfaction</li> <br> <li>Career Management, </li> <br> 
              <li>Accredited Learning, </li> <br> <li>Career Progression </li></ul> <br> Rule = OutcomeEffectiveDate >= 
              Session.DateAndTimeOfSession AND <= Session.DateAndTimeOfSession + 12 months <br> <br> When OutcomeType of: <br> 
              <br><ul><li>Sustainable Employment </li> </ul><br> Rule = OutcomeEffectiveDate >= Session.DateAndTimeOfSession 
              AND <= Session.DateAndTimeOfSession + 13 months <br> <br><b>ClaimedPriorityGroup:</b> This is mandatory if OutcomeClaimedDate has a value"
        )]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Outcomes/{outcomeId}")] HttpRequest req, string customerId, string interactionId, string actionplanId, string outcomeId)
        {
            _logger.LogInformation($"Function {nameof(PatchOutcomesHttpTrigger)} has been invoked");

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation($"Unable to locate 'TouchpointId' in request header. Correlation GUID: {correlationGuid}");
                return new BadRequestObjectResult("Unable to locate 'TouchpointId' in request header.");
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                _logger.LogInformation($"Unable to locate 'SubcontractorId' in request header. Correlation GUID: {correlationGuid}");
                return new BadRequestObjectResult("Unable to locate 'SubcontractorId' in request header");
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                _logger.LogInformation($"Unable to locate 'apimurl' in request header. Correlation GUID: {correlationGuid}");
                return new BadRequestObjectResult("Unable to locate 'apimurl' in request header");
            }

            _logger.LogInformation($"Header validation successful. Associated Touchpoint ID: {touchpointId}");

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogInformation($"Unable to parse 'customerId' to a GUID. Customer ID: {customerId}");
                return new BadRequestObjectResult(customerGuid);
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _logger.LogInformation($"Unable to parse 'interactionId' to a GUID. Interaction ID: {interactionId}");
                return new BadRequestObjectResult(interactionGuid);
            }

            if (!Guid.TryParse(actionplanId, out var actionPlanGuid))
            {
                _logger.LogInformation($"Unable to parse 'actionPlanId' to a GUID. Action Plan ID: {actionplanId}");
                return new BadRequestObjectResult(actionPlanGuid);
            }

            if (!Guid.TryParse(outcomeId, out var outcomesGuid))
            {
                _logger.LogInformation($"Unable to parse 'outcomeId' to a GUID. Outcome ID: {outcomeId}");
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
                _logger.LogError($"Unable to read request body. Correlation GUID: {correlationGuid}", ex);
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

            _logger.LogInformation("Attempting to get resource from body of the request.");

            try
            {
                outcomesPatchRequest = await _httpRequestHelper.GetResourceFromRequest<Models.OutcomesPatch>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve request body. Correlation GUID: {correlationGuid}", ex);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ExceptionToExclude));
            }

            if (outcomesPatchRequest == null)
            {
                _logger.LogInformation($"Outcome patch request is NULL. Correlation GUID: {correlationGuid}");
                return new UnprocessableEntityObjectResult(req);
            }

            _logger.LogInformation($"Attempting to set IDs for Outcome PATCH. Correlation GUID: {correlationGuid}");
            outcomesPatchRequest.SetIds(touchpointId, subcontractorId);
            _logger.LogInformation($"IDs successfully set for Outcome PATCH. Correlation GUID: {correlationGuid}");

            _logger.LogInformation($"Attempting to see if customer exists. Customer GUID: {customerGuid}");
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogInformation($"Customer does not exist. Customer GUID: {customerGuid}");
                return new NoContentResult();
            }
            else
            {
                _logger.LogInformation($"Customer does exist. Customer GUID: {customerGuid}");
            }

            _logger.LogInformation($"Attempting to see if this customer ({customerGuid}) is read-only");
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            _logger.LogInformation($"Attempting to see if this customer ({customerGuid}) has a termination reason");
            var isADuplicateCustomer = _resourceHelper.GetCustomerReasonForTermination();

            if (isCustomerReadOnly && isADuplicateCustomer != 3)
            {
                _logger.LogInformation($"Customer ({customerGuid}) is read-only");
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            if (isADuplicateCustomer == 3)
            {
                bool customerIsDuplicate = requestCount > 2 || requestCount == 1 && !setOutcomeClaimedDateToNull && !setOutcomeEffectiveDateToNull || (requestCount == 2 && (!setOutcomeClaimedDateToNull || !setOutcomeEffectiveDateToNull));
                if (customerIsDuplicate)
                {
                    _logger.LogInformation($"Duplicate Customer ({customerGuid}): This resource is read only. You may only remove values for Outcome Claimed and Effective date. Correlation GUID: {correlationGuid}");

                    return new ObjectResult(new HttpErrorResponse(new List<string> {"Duplicate Customer: This resource is read only. You may only remove values for Outcome Claimed and Effective date"}, correlationGuid))
                    {
                        StatusCode = (int)HttpStatusCode.Forbidden
                    };
                }
            }

            _logger.LogInformation($"Attempting to get Interaction ({interactionGuid}) for Customer ({customerGuid})");
            var doesInteractionExist = _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _logger.LogInformation($"Interaction does not exist. Interaction GUID: {interactionGuid}");
                return new NoContentResult();
            } 
            else
            {
                _logger.LogInformation($"Interaction does exist. Interaction GUID: {interactionGuid}");
            }

            _logger.LogInformation($"Attempting to get Action Plan ({actionPlanGuid}) for Customer ({customerGuid})");
            var doesActionPlanExist = _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
            {
                _logger.LogInformation($"Action Plan does not exist. Action Plan GUID: {actionPlanGuid}");
                return new NoContentResult();
            } 
            else
            {
                _logger.LogInformation($"Action Plan does exist. Action Plan GUID: {actionPlanGuid}");
            }

            _logger.LogInformation($"Attempting to get Outcome ({outcomesGuid}) for Customer ({customerGuid})");
            var outcome = await _outcomesPatchService.GetOutcomesForCustomerAsync(customerGuid, interactionGuid, actionPlanGuid, outcomesGuid);

            if (outcome == null)
            {
                _logger.LogInformation($"Outcome does not exist. Outcome GUID: {outcomesGuid}");
                return new NoContentResult();
            }
            else
            {
                _logger.LogInformation($"Outcome does exist. Outcome GUID: {outcomesGuid}");
            }

            _logger.LogInformation("Attempting to PATCH Outcome resource.");
            var patchedOutcomeResource = _outcomesPatchService.PatchResource(outcome, outcomesPatchRequest);

            if (patchedOutcomeResource == null)
            {
                _logger.LogInformation("Failed to PATCH Outcome resource.");
                return new NoContentResult();
            }

            if (setOutcomeClaimedDateToNull || setOutcomeEffectiveDateToNull)
            {
                _logger.LogInformation("Attempting to PATCH Outcome resource (OutcomeClaimedDate & OutcomeEffectiveDate).");

                patchedOutcomeResource =
                    _outcomesPatchService.UpdateOutcomeClaimedDateOutcomeEffectiveDateValue(patchedOutcomeResource,
                        setOutcomeClaimedDateToNull, setOutcomeEffectiveDateToNull);
            }

            Models.Outcomes outcomeValidationObject;

            _logger.LogInformation("Attempting to deserialize the PATCH Outcome resource.");

            try
            {
                outcomeValidationObject = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomeResource);
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Failure deserializing the PATCH Outcome resource. Correlation GUID: {correlationGuid}", ex);
                throw;
            }

            if (outcomeValidationObject == null)
            {
                _logger.LogInformation($"Outcome validation object is NULL. Correlation GUID: {correlationGuid}");
                return new UnprocessableEntityObjectResult(req);
            }

            _logger.LogInformation($"Attempting to get GetDateAndTimeOfSession for Session. Session ID: {outcomeValidationObject.SessionId}");
            var dateAndTimeOfSession = await _resourceHelper.GetDateAndTimeOfSession(outcomeValidationObject.SessionId.GetValueOrDefault());

            _logger.LogInformation($"Attempting to validate resource. Correlation GUID: {correlationGuid}");
            var errors = _validate.ValidateResource(outcomeValidationObject, dateAndTimeOfSession);

            if (errors != null && errors.Any())
            {
                _logger.LogInformation("Validation errors present with the resource.");
                return new UnprocessableEntityObjectResult(errors);
            }

            _logger.LogInformation($"Attempting to PATCH Outcome in Cosmos DB. Outcome GUID: {outcomesGuid}");
            var updatedOutcome = await _outcomesPatchService.UpdateCosmosAsync(patchedOutcomeResource, outcomesGuid);

            if (updatedOutcome != null)
            {
                _logger.LogInformation($"Successfully PATCHed Outcome in Cosmos DB. Outcome GUID: {outcomesGuid}");
                _logger.LogInformation($"Attempting to send message to Service Bus Namespace. Outcome GUID: {outcomesGuid}");
                await _outcomesPatchService.SendToServiceBusQueueAsync(updatedOutcome, customerGuid, apimUrl);
            }

            _logger.LogInformation($"Function {nameof(PatchOutcomesHttpTrigger)} has finished invocation");

            if (updatedOutcome == null)
                return new BadRequestObjectResult(outcomesGuid);

            return new JsonResult(updatedOutcome, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}
