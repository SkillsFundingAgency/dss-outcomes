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
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Validation;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using System.Text;

namespace NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Function
{
    public class PostOutcomesHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IPostOutcomesHttpTriggerService _outcomesPostService;
        private readonly IJsonHelper _jsonHelper;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IValidate _validate;
        private readonly ILogger log;
        public PostOutcomesHttpTrigger(IResourceHelper resourceHelper,
            IHttpRequestHelper httpRequestHelper,
            IPostOutcomesHttpTriggerService outcomesPostService,
            IJsonHelper jsonHelper,
            ILoggerHelper loggerHelper,
            IValidate validate,
            ILogger<PostOutcomesHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestHelper = httpRequestHelper;
            _outcomesPostService = outcomesPostService;
            _jsonHelper = jsonHelper;
            _loggerHelper = loggerHelper;
            _validate = validate;
            log = logger;
        }
        [Function("Post")]
        [ProducesResponseType(typeof(Models.Outcomes),200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Outcome Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Outcome does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Outcome validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new Outcome for a customer. <br>" +
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
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Outcomes")]HttpRequest req, string customerId, string interactionId, string actionplanId)
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
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'SubcontractorId' in request header");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid,
                string.Format("Post Outcome C# HTTP trigger function  processed a request. By Touchpoint: {0}",
                    touchpointId));

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'customerId' to a Guid: {0}", customerId));
                return new BadRequestObjectResult(new StringContent(JsonConvert.SerializeObject(customerGuid), Encoding.UTF8, ContentApplicationType.ApplicationJSON));
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'interactionId' to a Guid: {0}", interactionId));
                return new BadRequestObjectResult(new StringContent(JsonConvert.SerializeObject(interactionGuid), Encoding.UTF8, ContentApplicationType.ApplicationJSON));
            }

            if (!Guid.TryParse(actionplanId, out var actionplanGuid))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'actionplanId' to a Guid: {0}", actionplanId));
                return new BadRequestObjectResult(new StringContent(JsonConvert.SerializeObject(actionplanGuid), Encoding.UTF8, ContentApplicationType.ApplicationJSON));
            }

            Models.Outcomes outcomesRequest;

            try
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to get resource from body of the request");
                outcomesRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Outcomes>(req);
            }
            catch (JsonException ex)
            {
                _loggerHelper.LogError(log, correlationGuid, "Unable to retrieve body from req", ex);
                return new UnprocessableEntityObjectResult(new StringContent(JsonConvert.SerializeObject(ex), Encoding.UTF8,
                    ContentApplicationType.ApplicationJSON));
            }

            if (outcomesRequest == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Outcome request is null");
                return new UnprocessableEntityObjectResult(new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8,
                    ContentApplicationType.ApplicationJSON));
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to set id's for outcome");
            outcomesRequest.SetIds(customerGuid, actionplanGuid, touchpointId, subcontractorId);

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer does not exist {0}", customerGuid));
                return new NoContentResult();
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if this is a read only customer {0}", customerGuid));
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer is read only {0}", customerGuid));
                return new ForbidResult(new StringContent(JsonConvert.SerializeObject(customerGuid),
                    Encoding.UTF8, ContentApplicationType.ApplicationJSON).ToString());
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Interaction {0} for customer {1}", interactionGuid, customerGuid));
            var doesInteractionExist = _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Interaction does not exist {0}", interactionGuid));
                return new NoContentResult();
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get GetDateAndTimeOfSession for Session {0}", outcomesRequest.SessionId));
            var dateAndTimeOfSession = await _resourceHelper.GetDateAndTimeOfSession(outcomesRequest.SessionId.GetValueOrDefault());

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get ActionPlan {0} for customer {1}", interactionGuid, customerGuid));
            var doesActionPlanExist = _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionplanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("ActionPlan does not exist {0}", actionplanGuid));
                return new NoContentResult();
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = _validate.ValidateResource(outcomesRequest, dateAndTimeOfSession);

            if (errors != null && errors.Any())
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "validation errors with resource");
                return new UnprocessableEntityObjectResult(new StringContent(JsonConvert.SerializeObject(errors),
                    Encoding.UTF8, ContentApplicationType.ApplicationJSON));
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to Create Outcome for customer {0}", customerGuid));
            var outcome = await _outcomesPostService.CreateAsync(outcomesRequest);

            if (outcome != null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("attempting to send to service bus {0}", outcome.OutcomeId));
                await _outcomesPostService.SendToServiceBusQueueAsync(outcome, apimUrl);
            }

            _loggerHelper.LogMethodExit(log);

            return outcome == null
                ? new BadRequestObjectResult(new StringContent(JsonConvert.SerializeObject(customerGuid), Encoding.UTF8, ContentApplicationType.ApplicationJSON))
                : new ObjectResult(new StringContent(_jsonHelper.SerializeObjectAndRenameIdProperty(outcome, "id", "OutcomeId"), Encoding.UTF8,
                    ContentApplicationType.ApplicationJSON))
                {
                    StatusCode = StatusCodes.Status201Created
                };
        }
    }
}