using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Function
{
    public class PostOutcomesHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IPostOutcomesHttpTriggerService _outcomesPostService;
        private readonly IValidate _validate;
        private readonly ILogger<PostOutcomesHttpTrigger> _logger;
        private readonly IDynamicHelper _dynamicHelper;
        private static readonly string[] ExceptionToExclude = { "TargetSite" };

        public PostOutcomesHttpTrigger(IResourceHelper resourceHelper,
            IHttpRequestHelper httpRequestHelper,
            IPostOutcomesHttpTriggerService outcomesPostService,
            IValidate validate,
            ILogger<PostOutcomesHttpTrigger> logger,
            IDynamicHelper dynamicHelper)
        {
            _resourceHelper = resourceHelper;
            _httpRequestHelper = httpRequestHelper;
            _outcomesPostService = outcomesPostService;
            _validate = validate;
            _logger = logger;
            _dynamicHelper = dynamicHelper;
        }

        [Function("Post")]
        [ProducesResponseType(typeof(Models.Outcomes), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Outcome Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Customer, action plan or interaction do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Outcome validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description =
            @"Ability to create a new Outcome for a customer. <br> <br> <b>Validation Rules:</b> <br> 
               <br><b>OutcomeClaimedDate:</b> OutcomeClaimedDate >= OutcomeEffectiveDate <br> <br>
               <b>OutcomeEffectiveDate:</b> <br> When OutcomeType of: <br> <ul><li>Customer Satisfaction</li> 
               <br> <li>Career Management, </li> <br> <li>Accredited Learning, </li> <br> <li>Career Progression </li>
               </ul> <br> Rule = OutcomeEffectiveDate >= Session.DateAndTimeOfSession AND <= Session.DateAndTimeOfSession + 12 months <br>
               <br> When OutcomeType of: <br> <br><ul><li>Sustainable Employment </li> </ul><br> Rule = OutcomeEffectiveDate >= Session.DateAndTimeOfSession 
               AND <= Session.DateAndTimeOfSession + 13 months <br> <br><b>ClaimedPriorityGroup:</b> This is mandatory if OutcomeClaimedDate has a value"
        )]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Outcomes")] HttpRequest req, string customerId, string interactionId, string actionplanId)
        {
            _logger.LogTrace("Function {FunctionName} has been invoked", nameof(PostOutcomesHttpTrigger));

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation("Unable to locate 'TouchpointId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult("Unable to locate 'TouchpointId' in request header.");
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                _logger.LogInformation("Unable to locate 'SubcontractorId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult("Unable to locate 'SubcontractorId' in request header");
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                _logger.LogInformation("Unable to locate 'apimURL' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult("Unable to locate 'apimurl' in request header");
            }

            _logger.LogTrace("Header validation successful. Associated Touchpoint ID: {TouchpointId}", touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogInformation("Unable to parse 'customerId' to a GUID. Customer ID: {CustomerId}", customerId);
                return new BadRequestObjectResult("Unable to parse 'customerId' to a GUID. Customer ID: " +customerGuid);
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _logger.LogInformation("Unable to parse 'interactionId' to a GUID. Interaction ID: {InteractionId}", interactionId);
                return new BadRequestObjectResult("Unable to parse 'interactionId' to a GUID. Interaction ID: " + interactionGuid);
            }

            if (!Guid.TryParse(actionplanId, out var actionplanGuid))
            {
                _logger.LogInformation("Unable to parse 'actionPlanId' to a GUID. Action Plan ID: {ActionPlanId}", actionplanId);
                return new BadRequestObjectResult("Unable to parse 'actionPlanId' to a GUID. Action Plan ID: " + actionplanGuid);
            }

            Models.Outcomes outcomesRequest;

            _logger.LogTrace("Attempting to retrieve resource from request body. Correlation GUID: {CorrelationGuid}", correlationGuid);

            try
            {
                outcomesRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Outcomes>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve request body. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new UnprocessableEntityObjectResult($"Unable to retrieve request body. Correlation GUID: {correlationGuid}" + _dynamicHelper.ExcludeProperty(ex, ExceptionToExclude));
            }

            if (outcomesRequest == null)
            {
                _logger.LogInformation("Outcome post request is NULL. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new UnprocessableEntityObjectResult($"Outcome post request is NULL. Correlation GUID: {correlationGuid}");
            }

            _logger.LogTrace("Attempting to set IDs for Outcome POST. Correlation GUID: {CorrelationGuid}", correlationGuid);
            outcomesRequest.SetIds(customerGuid, actionplanGuid, touchpointId, subcontractorId);
            _logger.LogTrace("IDs successfully set for Outcome POST. Correlation GUID: {CorrelationGuid}", correlationGuid);

            _logger.LogTrace("Attempting to check if customer exists. Customer GUID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogInformation("Customer does not exist. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new NotFoundObjectResult("Failed to POST outcome. Customer does not exist. Customer GUID: " + customerGuid);
            }
            _logger.LogTrace("Customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            _logger.LogTrace("Attempting to check if customer is read-only. Customer GUID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                _logger.LogInformation("Customer is read-only. Customer GUID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            _logger.LogTrace("Attempting to get Interaction for Customer. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);
            var doesInteractionExist = await _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _logger.LogInformation("Interaction does not exist. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);
                return new NotFoundObjectResult("Failed to POST outcome. Interaction does not exist. Interaction GUID: " + interactionGuid);
            }
            _logger.LogTrace("Interaction exists. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);

            var doesSessionIsValid = await _resourceHelper.DoesSessionExistAndBelongToCustomer(outcomesRequest.SessionId.GetValueOrDefault(), interactionGuid, customerGuid);
            if (!doesSessionIsValid) {
                _logger.LogInformation("Session does not exist. Customer GUID: {CustomerId}. Session GUID: {SessionId}. Correlation GUID: {CorrelationGuid}", customerGuid, outcomesRequest.SessionId, correlationGuid);
                return new NotFoundObjectResult("Failed to POST outcome. Session does not exist. Session GUID: " + outcomesRequest.SessionId);
            }

            var doesSessionBelongsToActionPlan= await _resourceHelper.DoesSessionExistAndBelongToCustomerActionPlan(outcomesRequest.SessionId.GetValueOrDefault(), interactionGuid,actionplanGuid, customerGuid);
            if (!doesSessionBelongsToActionPlan)
            {
                _logger.LogInformation("Session does not belong to ActionPlan. Customer GUID: {CustomerId}. Session GUID: {SessionId}. Correlation GUID: {CorrelationGuid}  ActionPlan: {ActionPlan}", customerGuid, outcomesRequest.SessionId, correlationGuid,actionplanGuid);
                return new NotFoundObjectResult("Failed to POST outcome. Session does not belong to ActionPlan: " + outcomesRequest.SessionId);
            }
         
             _logger.LogTrace("Attempting to get DateAndTimeOfSession for Session. Session ID: {SessionId}", outcomesRequest.SessionId);
            var dateAndTimeOfSession = await _resourceHelper.GetDateAndTimeOfSession(outcomesRequest.SessionId.GetValueOrDefault());
            _logger.LogTrace("Successfully retrieved DateAndTimeOfSession for Session. {DateAndTimeOfSession}", dateAndTimeOfSession);
  
              _logger.LogTrace("Attempting to get Action Plan for Customer. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionplanGuid, correlationGuid);

            var doesActionPlanExist = await _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionplanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
            {
                _logger.LogInformation("Action Plan does not exist. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionplanGuid, correlationGuid);
                return new NotFoundObjectResult("Failed to POST outcome. Action Plan does not exist. Action Plan GUID: " + actionplanGuid);
            }
            _logger.LogTrace("Action Plan exists. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionplanGuid, correlationGuid);


            _logger.LogTrace("Attempting to validate {OutcomesRequest} object", nameof(outcomesRequest));
            var errors = _validate.ValidateResource(outcomesRequest, dateAndTimeOfSession);

            if (errors != null && errors.Any())
            {
                _logger.LogWarning("Failed to validate {OutcomesRequest}", nameof(outcomesRequest));
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogTrace("Successfully validated {OutcomesRequest}", nameof(outcomesRequest));

            var doesOutcomeExist = await _resourceHelper.DoesClaimedOutcomeExistForCustomerAsync(customerGuid, outcomesRequest.SessionId.Value, actionplanGuid, outcomesRequest.OutcomeType.Value);
            if (doesOutcomeExist)
            {
                _logger.LogInformation("Outcome for Customer GUID: {CustomerId} already exists for outcome type of: {outcomeType}", customerId, outcomesRequest.OutcomeType);
                return new BadRequestObjectResult("Failed to POST outcome. Outcome of type '"+ outcomesRequest.OutcomeType + "' already exists for customer ID: " + customerId);
            }

            _logger.LogTrace("Attempting to POST Outcome in Cosmos DB. Customer GUID: {CustomerGuid}", customerGuid);
            var outcome = await _outcomesPostService.CreateAsync(outcomesRequest);

            if (outcome != null)
            {
                _logger.LogTrace("Successfully POSTed Outcome in Cosmos DB. Outcome GUID: {OutcomeId}", outcome.OutcomeId);
                _logger.LogTrace("Attempting to send message to Service Bus Namespace. Outcome GUID: {OutcomeId}", outcome.OutcomeId);
                await _outcomesPostService.SendToServiceBusQueueAsync(outcome, apimUrl);

                _logger.LogTrace("Successfully sent message to Service Bus. Outcome GUID: {OutcomeId}", outcome.OutcomeId);
            }

            if (outcome == null)
            {
                _logger.LogInformation("POST request unsuccessful. Customer GUID: {CustomerGuid}", customerGuid);
                return new BadRequestObjectResult("Failed to POST outcome in Cosmos DB for customer " + customerGuid + ". Outcome is NULL after creation attempt.");
            }

            _logger.LogTrace("Function {FunctionName} has finished invoking", nameof(PostOutcomesHttpTrigger));
            return new JsonResult(outcome, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }
    }
}
