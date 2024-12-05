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
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Outcome does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
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
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(PostOutcomesHttpTrigger));

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
                _logger.LogWarning("Unable to locate 'TouchpointId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult("Unable to locate 'TouchpointId' in request header.");
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                _logger.LogWarning("Unable to locate 'SubcontractorId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult("Unable to locate 'SubcontractorId' in request header");
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                _logger.LogWarning("Unable to locate 'apimURL' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult("Unable to locate 'apimurl' in request header");
            }

            _logger.LogInformation("Header validation successful. Associated Touchpoint ID: {TouchpointId}", touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to parse 'customerId' to a GUID. Customer GUID: {CustomerID}", customerId);
                return new BadRequestObjectResult(customerGuid);
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _logger.LogWarning("Unable to parse 'interactionId' to a GUID. Interaction ID: {InteractionId}", interactionId);
                return new BadRequestObjectResult(interactionGuid);
            }

            if (!Guid.TryParse(actionplanId, out var actionplanGuid))
            {
                _logger.LogWarning("Unable to parse 'actionPlanId' to a GUID. Action Plan ID: {ActionplanId}", actionplanId);
                return new BadRequestObjectResult(actionplanGuid);
            }

            Models.Outcomes outcomesRequest;

            _logger.LogInformation("Attempting to retrieve resource from request body. Correlation GUID: {CorrelationGuid}", correlationGuid);

            try
            {
                outcomesRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Outcomes>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve request body. Correlation GUID: {CorrelationGuid}. Exception: {ExceptionMessage}", correlationGuid, ex.Message);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ExceptionToExclude));
            }

            if (outcomesRequest == null)
            {
                _logger.LogWarning("{outcomesRequest} object is NULL. Correlation GUID: {CorrelationGuid}", nameof(outcomesRequest), correlationGuid);
                return new UnprocessableEntityObjectResult(req);
            }

            _logger.LogInformation("Attempting to set IDs for Outcome POST. Correlation GUID: {CorrelationGuid}", correlationGuid);
            outcomesRequest.SetIds(customerGuid, actionplanGuid, touchpointId, subcontractorId);
            _logger.LogInformation("IDs successfully set for Outcome POST. Correlation GUID: {CorrelationGuid}", correlationGuid);

            _logger.LogInformation("Attempting to check if customer exists. Customer GUID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer does not exist. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            _logger.LogInformation("Attempting to check if customer is read-only. Customer GUID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                _logger.LogWarning("Customer is read-only. Customer GUID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            _logger.LogInformation("Attempting to get Interaction for Customer. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);
            var doesInteractionExist = await _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _logger.LogWarning("Interaction does not exist. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Interaction exists. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);


            _logger.LogInformation("Attempting to get DateAndTimeOfSession for Session. Session ID: {SessionId}", outcomesRequest.SessionId);
            var dateAndTimeOfSession = await _resourceHelper.GetDateAndTimeOfSession(outcomesRequest.SessionId.GetValueOrDefault());
            _logger.LogInformation("Successfully retrieved DateAndTimeOfSession for Session. {dateAndTimeOfSession}", dateAndTimeOfSession);

            _logger.LogInformation("Attempting to get Action Plan for Customer. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionplanGuid, correlationGuid);
            var doesActionPlanExist = await _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionplanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
            {
                _logger.LogWarning("Action Plan does not exist. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionplanGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Action Plan exists. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionplanGuid, correlationGuid);


            _logger.LogInformation("Attempting to validate {outcomesRequest} object", nameof(outcomesRequest));
            var errors = _validate.ValidateResource(outcomesRequest, dateAndTimeOfSession);

            if (errors != null && errors.Any())
            {
                _logger.LogWarning("Falied to validate {outcomesRequest}", nameof(outcomesRequest));
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogInformation("Successfully validated {outcomesRequest}", nameof(outcomesRequest));

            _logger.LogInformation("Attempting to POST Outcome in Cosmos DB. Customer GUID: {CustomerGuid}", customerGuid);
            var outcome = await _outcomesPostService.CreateAsync(outcomesRequest);

            if (outcome != null)
            {
                _logger.LogInformation("Successfully POSTed Outcome in Cosmos DB. Outcome GUID: {OutcomeId}", outcome.OutcomeId);
                _logger.LogInformation("Attempting to send message to Service Bus Namespace. Outcome GUID: {OutcomeId}", outcome.OutcomeId);
                await _outcomesPostService.SendToServiceBusQueueAsync(outcome, apimUrl);

                _logger.LogInformation("Successfully sent message to Service Bus. Outcome GUID: {OutcomeId}", outcome.OutcomeId);
            }

            if (outcome == null)
            {
                _logger.LogWarning("POST request unsuccessful. Customer GUID: {CustomerGuid}", customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PostOutcomesHttpTrigger));
                return new BadRequestObjectResult(customerGuid);
            }

            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PostOutcomesHttpTrigger));
            return new JsonResult(outcome, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }
    }
}