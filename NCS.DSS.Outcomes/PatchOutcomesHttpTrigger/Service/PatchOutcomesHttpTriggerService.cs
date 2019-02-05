using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.Models;
using NCS.DSS.Outcomes.ServiceBus;
using Newtonsoft.Json;

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

        public async Task<Models.Outcomes> UpdateAsync(string outcomeJson, OutcomesPatch outcomesPatch, Guid outcomeId)
        {
            if (string.IsNullOrEmpty(outcomeJson))
                return null;

            if (outcomesPatch == null)
                return null;

            outcomesPatch.SetDefaultValues();

            var updatedJson = _outcomePatchService.Patch(outcomeJson, outcomesPatch);

            if (string.IsNullOrEmpty(updatedJson))
                return null;

            var response = await _documentDbProvider.UpdateOutcomesAsync(updatedJson, outcomeId);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<Models.Outcomes>(updatedJson) : null;
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