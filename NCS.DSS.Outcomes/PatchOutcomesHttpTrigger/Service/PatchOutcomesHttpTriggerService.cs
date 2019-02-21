using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.Models;
using NCS.DSS.Outcomes.ServiceBus;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service
{
    public class PatchOutcomesHttpTriggerService : IPatchOutcomesHttpTriggerService
    {

        private readonly IOutcomePatchService _outcomePatchService;
        private readonly IDocumentDBProvider _documentDbProvider;

        public PatchOutcomesHttpTriggerService(IDocumentDBProvider documentDbProvider, IOutcomePatchService outcomePatchService)
        {
            _documentDbProvider = documentDbProvider;
            _outcomePatchService = outcomePatchService;
        }

        public string PatchResource(string outcomeJson, OutcomesPatch outcomesPatchPatch)
        {
            if (string.IsNullOrEmpty(outcomeJson))
                return null;

            if (outcomesPatchPatch == null)
                return null;

            outcomesPatchPatch.SetDefaultValues();

            var updatedOutcome = _outcomePatchService.Patch(outcomeJson, outcomesPatchPatch);

            return updatedOutcome;
        }

        public async Task<Models.Outcomes> UpdateCosmosAsync(string outcomeJson, Guid outcomeId)
        {
            if (string.IsNullOrEmpty(outcomeJson))
                return null;

            var response = await _documentDbProvider.UpdateOutcomesAsync(outcomeJson, outcomeId);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? (dynamic)response.Resource : null;
        }

        public async Task<string> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionPlanId, Guid outcomeId)
        {
            var outcomes = await _documentDbProvider.GetOutcomesForCustomerAsyncToUpdateAsync(customerId, interactionsId, actionPlanId, outcomeId);

            return outcomes;
        }

        public async Task SendToServiceBusQueueAsync(Models.Outcomes outcomes, Guid customerId, string reqUrl)
        {
            await ServiceBusClient.SendPatchMessageAsync(outcomes, customerId, reqUrl);
        }
    }
}