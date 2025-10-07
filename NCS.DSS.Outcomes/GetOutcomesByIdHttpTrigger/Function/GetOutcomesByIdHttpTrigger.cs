using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Function
{
    public class GetOutcomesByIdHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IGetOutcomesByIdHttpTriggerService _outcomesGetService;
        private readonly ILogger<GetOutcomesByIdHttpTrigger> _logger;

        public GetOutcomesByIdHttpTrigger(IResourceHelper resourceHelper,
            IHttpRequestHelper httpRequestHelper,
            IGetOutcomesByIdHttpTriggerService outcomesGetService,
            ILogger<GetOutcomesByIdHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestHelper = httpRequestHelper;
            _outcomesGetService = outcomesGetService;
            _logger = logger;
        }

        [Function("GetById")]
        [ProducesResponseType(typeof(Models.Outcomes), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Outcome found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Outcome, customer, action plan or interaction do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual Outcome for the given customer")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Outcomes/{outcomeId}")] HttpRequest req, string customerId, string interactionId, string actionplanId, string outcomeId)
        {
            _logger.LogTrace("Function {FunctionName} has been invoked", nameof(GetOutcomesByIdHttpTrigger));

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
                return new BadRequestObjectResult("Unable to locate 'TouchpointId' in request header");
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                _logger.LogInformation("Unable to locate 'SubcontractorId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult("Unable to locate 'SubcontractorId' in request header");
            }

            _logger.LogTrace("Header validation successful. Associated Touchpoint ID: {TouchpointId}", touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogInformation("Unable to parse 'customerId' to a GUID. Customer ID: {CustomerId}", customerId);
                return new BadRequestObjectResult("Unable to parse 'customerId' to a GUID. Customer ID: " + customerId);
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _logger.LogInformation("Unable to parse 'interactionId' to a GUID. Interaction ID: {InteractionId}", interactionId);
                return new BadRequestObjectResult("Unable to parse 'interactionId' to a GUID. Interaction ID: " + interactionGuid);
            }

            if (!Guid.TryParse(actionplanId, out var actionPlanGuid))
            {
                _logger.LogInformation("Unable to parse 'actionPlanId' to a GUID. Action Plan ID: {ActionPlanId}", actionplanId);
                return new BadRequestObjectResult("Unable to parse 'actionPlanId' to a GUID. Action Plan ID: " +  actionPlanGuid);
            }

            if (!Guid.TryParse(outcomeId, out var outcomesGuid))
            {
                _logger.LogInformation("Unable to parse 'outcomeId' to a GUID. Outcome ID: {OutcomeId}", outcomeId);
                return new BadRequestObjectResult("Unable to parse 'outcomeId' to a GUID. Outcome ID: " + outcomesGuid);
            }

            _logger.LogTrace("Attempting to check if customer exists. Customer GUID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogInformation("Failed to GET outcome. Customer does not exist. Customer GUID: {CustomerGuid}", customerGuid);
                return new NotFoundObjectResult($"Failed to GET outcome. Customer does not exist. Customer GUID: {customerGuid}");
            }
            _logger.LogTrace("Customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            _logger.LogTrace("Attempting to get Interaction for Customer. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);
            var doesInteractionExist = await _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _logger.LogInformation("Interaction does not exist. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);
                return new NotFoundObjectResult($"Failed to GET outcome. Interaction does not exist. Interaction GUID: {interactionGuid}");
            }

            _logger.LogTrace("Interaction exists. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);

            _logger.LogTrace("Attempting to get Action Plan for Customer. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);
            var doesActionPlanExist = await _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
            {
                _logger.LogInformation("Action Plan does not exist. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);
                return new NotFoundObjectResult($"Failed to GET outcome. Action Plan does not exist. Action Plan GUID: {actionPlanGuid}");
            }

            _logger.LogTrace("Action Plan exists. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);

            _logger.LogTrace("Attempting to get Outcome for Customer. Customer GUID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var outcomes = await _outcomesGetService.GetOutcomesForCustomerAsync(customerGuid, interactionGuid, actionPlanGuid, outcomesGuid);

            if (outcomes == null)
            {
                _logger.LogInformation("Outcome does not exist for Customer. Customer GUID: {CustomerGuid}", customerGuid);
                return new NotFoundObjectResult($"Failed to GET outcome. Outcome [{outcomesGuid}] for Customer [{customerGuid}] was not found");
            }

            _logger.LogTrace("Outcome successfully retrieved. Outcome GUID: {OutcomesGuid}", outcomesGuid);
            _logger.LogTrace("Function {FunctionName} has finished invoking", nameof(GetOutcomesByIdHttpTrigger));
            return new JsonResult(outcomes, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}