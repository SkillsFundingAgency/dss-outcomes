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
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Function
{
    public static class PatchOutcomesHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.Outcomes))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Action Plan Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action Plan does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Action Plan validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update a customers action plan record.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/actionplans/{actionplanId}/Outcomes/{OutcomesId}")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string actionplanId, string OutcomesId,
            [Inject]IResourceHelper resourceHelper, 
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPatchOutcomesHttpTriggerService OutcomesPatchService)
        {
            log.LogInformation("Patch Action Plan C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(actionplanId, out var actionplanGuid))
                return HttpResponseMessageHelper.BadRequest(actionplanGuid);

            if (!Guid.TryParse(OutcomesId, out var OutcomesGuid))
                return HttpResponseMessageHelper.BadRequest(OutcomesGuid);

            Models.OutcomesPatch  OutcomesPatchRequest;

            try
            {
                OutcomesPatchRequest = await httpRequestMessageHelper.GetOutcomesFromRequest<Models.OutcomesPatch>(req);
            }
            catch (JsonSerializationException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (OutcomesPatchRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            var errors = validate.ValidateResource(OutcomesPatchRequest);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var doesInteractionExist = resourceHelper.DoesInteractionExist(interactionGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var doesActionPlanExist = resourceHelper.DoesActionPlanExist(actionplanGuid);

            if (!doesActionPlanExist)
                return HttpResponseMessageHelper.NoContent(actionplanGuid);

            var Outcomes = await OutcomesPatchService.GetOutcomesForCustomerAsync(customerGuid, interactionGuid, actionplanGuid, OutcomesGuid);

            if (Outcomes == null)
                return HttpResponseMessageHelper.NoContent(OutcomesGuid);

            var updatedOutcomes = await OutcomesPatchService.UpdateAsync(Outcomes, OutcomesPatchRequest);

            return updatedOutcomes == null ?
                HttpResponseMessageHelper.BadRequest(OutcomesGuid) :
                HttpResponseMessageHelper.Ok(updatedOutcomes);

        }
    }
}
