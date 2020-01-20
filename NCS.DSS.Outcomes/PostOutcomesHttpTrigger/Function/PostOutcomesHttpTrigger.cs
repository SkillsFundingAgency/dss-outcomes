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
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Function
{
    public static class PostOutcomesHttpTrigger
    {
        [FunctionName("Post")]
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
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Outcomes")]HttpRequest req, ILogger log, string customerId, string interactionId, string actionplanId, 
            [Inject]IResourceHelper resourceHelper,
            [Inject]IPostOutcomesHttpTriggerService outcomesPostService,
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

            var apimUrl = httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return httpResponseMessageHelper.BadRequest();
            }

            var subcontractorId = httpRequestHelper.GetDssSubcontractorId(req);

            if (string.IsNullOrEmpty(subcontractorId))
                loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'SubcontractorId' in request header");

            loggerHelper.LogInformationMessage(log, correlationGuid,
                string.Format("Post Outcome C# HTTP trigger function  processed a request. By Touchpoint: {0}",
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

            if (!Guid.TryParse(actionplanId, out var actionplanGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'actionplanId' to a Guid: {0}", actionplanId));
                return httpResponseMessageHelper.BadRequest(actionplanGuid);
            }

            Models.Outcomes outcomesRequest;

            try
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to get resource from body of the request");
                outcomesRequest = await httpRequestHelper.GetResourceFromRequest<Models.Outcomes>(req);
            }
            catch (JsonException ex)
            {
                loggerHelper.LogError(log, correlationGuid, "Unable to retrieve body from req", ex);
                return httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (outcomesRequest == null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Outcome request is null");
                return httpResponseMessageHelper.UnprocessableEntity(req);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to set id's for outcome");
            outcomesRequest.SetIds(customerGuid, actionplanGuid, touchpointId, subcontractorId);

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer does not exist {0}", customerGuid));
                return httpResponseMessageHelper.NoContent(customerGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if this is a read only customer {0}", customerGuid));
            var isCustomerReadOnly = resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer is read only {0}", customerGuid));
                return httpResponseMessageHelper.Forbidden(customerGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Interaction {0} for customer {1}", interactionGuid, customerGuid));
            var doesInteractionExist = resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Interaction does not exist {0}", interactionGuid));
                return httpResponseMessageHelper.NoContent(interactionGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Session {0} for customer {1}", outcomesRequest.SessionId.GetValueOrDefault(), customerGuid));
            var doesSessionExist = resourceHelper.DoesSessionExistAndBelongToCustomer(outcomesRequest.SessionId.GetValueOrDefault(), interactionGuid, customerGuid);

            if (!doesSessionExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Session does not exist {0}", outcomesRequest.SessionId.GetValueOrDefault()));
                //return httpResponseMessageHelper.UnprocessableEntity(string.Format("Session ({0}) is not valid for interaction ({1}).", outcomesRequest.SessionId.GetValueOrDefault(), interactionGuid));
                return httpResponseMessageHelper.NoContent(outcomesRequest.SessionId.GetValueOrDefault());
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get GetDateAndTimeOfSession for Session {0}", outcomesRequest.SessionId));
            var dateAndTimeOfSession = resourceHelper.GetDateAndTimeOfSession(outcomesRequest.SessionId.GetValueOrDefault());

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get ActionPlan {0} for customer {1}", interactionGuid, customerGuid));
            var doesActionPlanExist = resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionplanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("ActionPlan does not exist {0}", actionplanGuid));
                return httpResponseMessageHelper.NoContent(actionplanGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = validate.ValidateResource(outcomesRequest, dateAndTimeOfSession);

            if (errors != null && errors.Any())
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "validation errors with resource");
                return httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to Create Outcome for customer {0}", customerGuid));
            var outcome = await outcomesPostService.CreateAsync(outcomesRequest);

            if (outcome != null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("attempting to send to service bus {0}", outcome.OutcomeId));
                await outcomesPostService.SendToServiceBusQueueAsync(outcome, apimUrl);
            }

            loggerHelper.LogMethodExit(log);

            return outcome == null
                ? httpResponseMessageHelper.BadRequest(customerGuid)
                : httpResponseMessageHelper.Created(jsonHelper.SerializeObjectAndRenameIdProperty(outcome, "id", "OutcomeId"));

        }
    }
}