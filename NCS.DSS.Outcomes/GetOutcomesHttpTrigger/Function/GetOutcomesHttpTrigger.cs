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
            _logger.LogInformation($"Function {nameof(GetOutcomesHttpTrigger)} has been invoked");

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
                return new BadRequestObjectResult("Unable to locate 'TouchpointId' in request header");
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                _logger.LogInformation($"Unable to locate 'SubcontractorId' in request header. Correlation GUID: {correlationGuid}");
                return new BadRequestObjectResult("Unable to locate 'SubcontractorId' in request header");
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

            _logger.LogInformation($"Attempting to get Outcomes for Customer ({customerGuid})");
            var outcomes = await _outcomesGetService.GetOutcomesAsync(customerGuid);

            _logger.LogInformation($"Function {nameof(GetOutcomesHttpTrigger)} has finished invocation");

            if (outcomes == null)
                return new NoContentResult();

            if (outcomes.Count == 1)
            {
                return new JsonResult(outcomes[0], new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK,
                };
            }
            else
            {
                return new JsonResult(outcomes, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK,
                };
            }
        }
    }
}