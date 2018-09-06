using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Annotations;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.Helpers;
using NCS.DSS.Outcomes.Ioc;
using NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Function
{
    public static class PostOutcomesHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Outcomes))]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Action Plan Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action Plan does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Action Plan validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new action plan for a customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/actionplans/{actionplanId}/Outcomes")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string actionplanId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPostOutcomesHttpTriggerService outcomesPostService)
        {
            var touchpointId = httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
                return HttpResponseMessageHelper.BadRequest();
            }

            var ApimURL = httpRequestMessageHelper.GetApimURL(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Post Action Plan C# HTTP trigger function processed a request. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(actionplanId, out var actionplanGuid))
                return HttpResponseMessageHelper.BadRequest(actionplanGuid);

            Models.Outcomes outcomesRequest;

            try
            {
                outcomesRequest = await httpRequestMessageHelper.GetOutcomesFromRequest<Models.Outcomes>(req);
            }
            catch (JsonException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (outcomesRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            outcomesRequest.SetIds(customerGuid, actionplanGuid, touchpointId);

            var errors = validate.ValidateResource(outcomesRequest);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return HttpResponseMessageHelper.Forbidden(customerGuid);

            var doesInteractionExist = resourceHelper.DoesInteractionExist(interactionGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var doesActionPlanExist = resourceHelper.DoesActionPlanExist(actionplanGuid);

            if (!doesActionPlanExist)
                return HttpResponseMessageHelper.NoContent(actionplanGuid);

            var outcomes = await outcomesPostService.CreateAsync(outcomesRequest);

            if (outcomes != null)
                await outcomesPostService.SendToServiceBusQueueAsync(outcomes, ApimURL);

            return outcomes == null
                ? HttpResponseMessageHelper.BadRequest(customerGuid)
                : HttpResponseMessageHelper.Created(JsonHelper.SerializeObject(outcomes));

        }
    }
}