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
            _logger.LogInformation($"Function {nameof(PostOutcomesHttpTrigger)} has been invoked");

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
                return new BadRequestObjectResult("Unable to parse 'customerId' to a GUID. Customer ID: " +customerGuid);
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _logger.LogInformation($"Unable to parse 'interactionId' to a GUID. Interaction ID: {interactionId}");
                return new BadRequestObjectResult("Unable to parse 'interactionId' to a GUID. Interaction ID: " + interactionGuid);
            }

            if (!Guid.TryParse(actionplanId, out var actionplanGuid))
            {
                _logger.LogInformation($"Unable to parse 'actionPlanId' to a GUID. Action Plan ID: {actionplanId}");
                return new BadRequestObjectResult("Unable to parse 'actionPlanId' to a GUID. Action Plan ID: " + actionplanGuid);
            }

            Models.Outcomes outcomesRequest;

            _logger.LogInformation($"Attempting to get resource from the request body. Correlation GUID: {correlationGuid}");

            try
            {
                outcomesRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Outcomes>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve request body. Correlation GUID: {correlationGuid}", ex);
                return new UnprocessableEntityObjectResult($"Unable to retrieve request body. Correlation GUID: {correlationGuid}" + _dynamicHelper.ExcludeProperty(ex, ExceptionToExclude));
            }

            if (outcomesRequest == null)
            {
                _logger.LogInformation($"Outcome post request is NULL. Correlation GUID: {correlationGuid}");
                return new UnprocessableEntityObjectResult($"Outcome post request is NULL. Correlation GUID: {correlationGuid}");
            }

            _logger.LogInformation($"Attempting to set IDs for Outcome POST. Correlation GUID: {correlationGuid}");
            outcomesRequest.SetIds(customerGuid, actionplanGuid, touchpointId, subcontractorId);
            _logger.LogInformation($"IDs successfully set for Outcome POST. Correlation GUID: {correlationGuid}");

            _logger.LogInformation($"Attempting to see if customer exists. Customer GUID: {customerGuid}");
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogInformation($"Failed to POST outcome. Customer does not exist. Customer GUID: {customerGuid}");
                return new NotFoundObjectResult("Failed to POST outcome. Customer does not exist. Customer GUID: " + customerGuid);
            }
            else
            {
                _logger.LogInformation($"Customer does exist. Customer GUID: {customerGuid}");
            }

            _logger.LogInformation($"Attempting to see if this customer ({customerGuid}) is read-only");
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                _logger.LogInformation($"Customer ({customerGuid}) is read-only");
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            _logger.LogInformation($"Attempting to get Interaction ({interactionGuid}) for Customer ({customerGuid})");
            var doesInteractionExist = _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _logger.LogInformation($"Failed to POST outcome. Interaction does not exist. Interaction GUID: {interactionGuid}");
                return new NotFoundObjectResult("Failed to POST outcome. Interaction does not exist. Interaction GUID: " + interactionGuid);
            }
            else
            {
                _logger.LogInformation($"Interaction does exist. Interaction GUID: {interactionGuid}");
            }

            _logger.LogInformation($"Attempting to get GetDateAndTimeOfSession for Session. Session ID: {outcomesRequest.SessionId}");
            var dateAndTimeOfSession = await _resourceHelper.GetDateAndTimeOfSession(outcomesRequest.SessionId.GetValueOrDefault());

            _logger.LogInformation($"Attempting to get Action Plan ({actionplanGuid}) for Customer ({customerGuid}). Interaction GUID: {interactionGuid}");
            var doesActionPlanExist = _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionplanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
            {
                _logger.LogInformation($"Failed to POST outcome. Action Plan does not exist. Action Plan GUID: {actionplanGuid}");
                return new NotFoundObjectResult("Failed to POST outcome. Action Plan does not exist. Action Plan GUID: " + actionplanGuid);
            }
            else
            {
                _logger.LogInformation($"Action Plan does exist. Action Plan GUID: {actionplanGuid}");
            }

            _logger.LogInformation($"Attempting to validate resource. Correlation GUID: {correlationGuid}");
            var errors = _validate.ValidateResource(outcomesRequest, dateAndTimeOfSession);

            if (errors != null && errors.Any())
            {
                _logger.LogInformation("Validation errors present with the resource.");
                return new UnprocessableEntityObjectResult(errors);
            }

            _logger.LogInformation($"Attempting to POST Outcome to Cosmos DB. Customer GUID: {customerGuid}");
            var outcome = await _outcomesPostService.CreateAsync(outcomesRequest);

            if (outcome != null)
            {
                _logger.LogInformation($"Successfully POSTed Outcome to Cosmos DB. Outcome ID: {outcome.OutcomeId}");
                _logger.LogInformation($"Attempting to send message to Service Bus Namespace. Outcome ID: {outcome.OutcomeId}");
                await _outcomesPostService.SendToServiceBusQueueAsync(outcome, apimUrl);
            }

            _logger.LogInformation($"Function {nameof(PostOutcomesHttpTrigger)} has finished invocation");

            if (outcome == null)
                return new BadRequestObjectResult("Failed to POST outcome in Cosmos DB for customer " + customerGuid + ". Outcome is NULL after creation attempt.");

            return new JsonResult(outcome, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }
    }
}