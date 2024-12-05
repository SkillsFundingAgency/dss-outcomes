using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Function
{
    public class GetOutcomesHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IGetOutcomesHttpTriggerService _outcomesGetService;
        private readonly ILogger<GetOutcomesHttpTrigger> _logger;

        public GetOutcomesHttpTrigger(IResourceHelper resourceHelper,
            IHttpRequestHelper httpRequestHelper,
            IGetOutcomesHttpTriggerService outcomesGetService,
            ILogger<GetOutcomesHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestHelper = httpRequestHelper;
            _outcomesGetService = outcomesGetService;
            _logger = logger;
        }
        [Function("Get")]
        [ProducesResponseType(typeof(Models.Outcomes), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Outcome found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Outcome does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return all Outcome for the given Interactions.")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Outcomes")] HttpRequest req, string customerId, string interactionId, string actionplanId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(GetOutcomesHttpTrigger));

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
                return new BadRequestObjectResult("Unable to locate 'TouchpointId' in request header");
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                _logger.LogWarning("Unable to locate 'SubcontractorId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult("Unable to locate 'SubcontractorId' in request header");
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

            if (!Guid.TryParse(actionplanId, out var actionPlanGuid))
            {
                _logger.LogWarning("Unable to parse 'actionPlanId' to a GUID. Action Plan ID: {ActionplanId}", actionplanId);
                return new BadRequestObjectResult(actionPlanGuid);
            }

            _logger.LogInformation("Attempting to check if customer exists. Customer GUID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer does not exist. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new NoContentResult();
            }

            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            _logger.LogInformation("Attempting to get Interaction for Customer. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);
            var doesInteractionExist = await _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _logger.LogWarning("Interaction does not exist. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);
                return new NoContentResult();
            }
            else
            {
                _logger.LogInformation("Interaction exists. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);
            }

            _logger.LogInformation("Attempting to get Action Plan for Customer. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);
            var doesActionPlanExist = await _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
            {
                _logger.LogWarning("Action Plan does not exist. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);
                return new NoContentResult();
            }
            else
            {
                _logger.LogInformation("Action Plan exists. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);
            }

            _logger.LogInformation("Attempting to get Outcomes for Customer. Customer GUID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var outcomes = await _outcomesGetService.GetOutcomesAsync(customerGuid);

            if (outcomes == null)
            {
                _logger.LogInformation("Outcome does not exist for Customer. Customer GUID: {CustomerGuid}", customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetOutcomesHttpTrigger));
                return new NoContentResult();
            }

            if (outcomes.Count == 1)
            {
                _logger.LogInformation("Outcome successfully retrieved. Outcome GUID: {OutcomeId} Customer GUID: {CustomerGuid}", outcomes.First().OutcomeId, customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetOutcomesHttpTrigger));
                return new JsonResult(outcomes[0], new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK,
                };
            }
            else
            {
                _logger.LogInformation("{Count} Outcomes successfully retrieved. Customer GUID: {CustomerGuid}", outcomes.Count, customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetOutcomesHttpTrigger));
                return new JsonResult(outcomes, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK,
                };
            }
        }
    }
}