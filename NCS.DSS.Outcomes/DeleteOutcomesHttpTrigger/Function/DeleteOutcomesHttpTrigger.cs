using DFC.Common.Standard.Logging;
using DFC.Functions.DI.Standard.Attributes;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.DeleteOutcomesHttpTrigger.Service;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using NCS.DSS.Outcomes.Validation;

namespace NCS.DSS.Outcomes.DeleteOutcomesHttpTrigger.Function
{
    public class DeleteOutcomesHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IDeleteOutcomesHttpTriggerService _outcomesDeleteService;
        private readonly IJsonHelper _jsonHelper;

        public DeleteOutcomesHttpTrigger(IResourceHelper resourceHelper,
            IHttpRequestHelper httpRequestHelper,
            IDeleteOutcomesHttpTriggerService outcomesDeleteService,
            IJsonHelper jsonHelper)
        {
            _resourceHelper = resourceHelper;
            _httpRequestHelper = httpRequestHelper;
            _outcomesDeleteService = outcomesDeleteService;
            _jsonHelper = jsonHelper;
        }
        [Disable]
        [Function("Delete")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Outcome deleted", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Outcome does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Delete", Description = "Ability to remove a customers Outcome record.")]
        public async Task<IActionResult> RunAsync([Microsoft.Azure.Functions.Worker.HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Customers/{customerId}/Interactions/{interactionId}/actionplans/{actionplanId}/Outcomes/{outcomeId}")] HttpRequest req, ILogger log, string customerId, string interactionId, string actionplanId, string outcomeId)
        {
            log.LogInformation("Delete Action Plan C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return new BadRequestObjectResult(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return new BadRequestObjectResult(interactionGuid);

            if (!Guid.TryParse(actionplanId, out var actionplanGuid))
                return new BadRequestObjectResult(actionplanGuid);

            if (!Guid.TryParse(outcomeId, out var outcomesGuid))
                return new BadRequestObjectResult(outcomesGuid);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return new NoContentResult();

            var doesActionPlanExist = _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionplanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
                return new NoContentResult();

            var outcome = await _outcomesDeleteService.GetOutcomeForCustomerAsync(customerGuid, interactionGuid, actionplanGuid, outcomesGuid);

            if (outcome == null)
                return new NoContentResult();

            var outcomeDeleted = await _outcomesDeleteService.DeleteAsync(outcome.OutcomeId.GetValueOrDefault());

            return !outcomeDeleted ?
                new BadRequestObjectResult(outcomesGuid) :
                new OkObjectResult(outcomesGuid);

        }
    }
}