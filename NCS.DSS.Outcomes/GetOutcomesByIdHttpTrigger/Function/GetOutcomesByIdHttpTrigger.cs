using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Mvc;
using DFC.Functions.DI.Standard.Attributes;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;

namespace NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Function
{
    public static class GetOutcomesByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [ProducesResponseType(typeof(Models.Outcomes), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Outcome found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Outcome does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual Outcome for the given customer")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Outcomes/{outcomeId}")]HttpRequest req, ILogger log, string customerId, string interactionId, string actionplanId, string outcomeId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IGetOutcomesByIdHttpTriggerService outcomesGetService,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IHttpRequestHelper httpRequestHelper,
            [Inject]IHttpResponseMessageHelper httpResponseMessageHelper,
            [Inject]IJsonHelper jsonHelper)
        {

            loggerHelper.LogMethodEnter(log);

            var correlationId = httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
            {
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");
            }

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'TouchpointId' in request header");
                return httpResponseMessageHelper.BadRequest();
            }

            var subcontractorId = httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'SubcontractorId' in request header");
                return httpResponseMessageHelper.BadRequest();
            }

            loggerHelper.LogInformationMessage(log, correlationGuid,
                string.Format("Get Outcomes By Id C# HTTP trigger function  processed a request. By Touchpoint: {0}",
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
            
            if (!Guid.TryParse(actionplanId, out var actionPlanGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'actionPlanId' to a Guid: {0}", actionplanId));
                return httpResponseMessageHelper.BadRequest(actionPlanGuid);
            }

            if (!Guid.TryParse(outcomeId, out var outcomesGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'outcomeId' to a Guid: {0}", outcomeId));
                return httpResponseMessageHelper.BadRequest(outcomesGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer does not exist {0}", customerGuid));
                return httpResponseMessageHelper.NoContent(customerGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Interaction {0} for customer {1}", interactionGuid, customerGuid));
            var doesInteractionExist = resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Interaction does not exist {0}", interactionGuid));
                return httpResponseMessageHelper.NoContent(interactionGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get ActionPlan {0} for customer {1}", actionPlanGuid, customerGuid));
            var doesActionPlanExist = resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("ActionPlan does not exist {0}", actionPlanGuid));
                return httpResponseMessageHelper.NoContent(actionPlanGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Outcome {0} for customer {1}", outcomesGuid, customerGuid));
            var outcomes = await outcomesGetService.GetOutcomesForCustomerAsync(customerGuid, interactionGuid, actionPlanGuid, outcomesGuid);

            loggerHelper.LogMethodExit(log);

            return outcomes == null ?
                httpResponseMessageHelper.NoContent(outcomesGuid) :
                httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectAndRenameIdProperty(outcomes, "id", "OutcomeId"));

        }
    }
}