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
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/sessions/{sessionId}/actionplans/{actionplanId}/Outcomes/{OutcomeId}")]HttpRequest req, ILogger log, string customerId, string interactionId, string actionplanId, string OutcomeId, string sessionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IGetOutcomesByIdHttpTriggerService outcomesGetService,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IHttpRequestHelper httpRequestHelper,
            [Inject]IHttpResponseMessageHelper httpResponseMessageHelper,
            [Inject]IJsonHelper jsonHelper)
        {

            return httpResponseMessageHelper.Ok();

            //var touchpointId = httpRequestHelper.GetDssTouchpointId(req);
            //if (string.IsNullOrEmpty(touchpointId))
            //{
            //    log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
            //    return httpResponseMessageHelper.BadRequest();
            //}

            //log.LogInformation("Get Outcomes By Id C# HTTP trigger function  processed a request. " + touchpointId);

            //if (!Guid.TryParse(customerId, out var customerGuid))
            //    return httpResponseMessageHelper.BadRequest(customerGuid);

            //if (!Guid.TryParse(interactionId, out var interactionGuid))
            //    return httpResponseMessageHelper.BadRequest(interactionGuid);

            //if (!Guid.TryParse(actionplanId, out var actionPlanGuid))
            //    return httpResponseMessageHelper.BadRequest(actionPlanGuid);

            //if (!Guid.TryParse(OutcomeId, out var outcomesGuid))
            //    return httpResponseMessageHelper.BadRequest(outcomesGuid);

            ////Check customer
            //var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            //if (!doesCustomerExist)
            //    return httpResponseMessageHelper.NoContent(customerGuid);

            //var doesInteractionExist = resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            //if (!doesInteractionExist)
            //    return httpResponseMessageHelper.NoContent(interactionGuid);

            //var doesActionPlanExist = resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            //if (!doesActionPlanExist)
            //    return httpResponseMessageHelper.NoContent(actionPlanGuid);

            //var outcomes = await outcomesGetService.GetOutcomesForCustomerAsync(customerGuid, interactionGuid, actionPlanGuid, outcomesGuid);

            //return outcomes == null ?
            //    httpResponseMessageHelper.NoContent(outcomesGuid) :
            //    httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectAndRenameIdProperty(outcomes, "id", "OutcomeId"));

        }
    }
}