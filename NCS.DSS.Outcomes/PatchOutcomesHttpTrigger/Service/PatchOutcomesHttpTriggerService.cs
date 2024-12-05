using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.Models;
using NCS.DSS.Outcomes.ServiceBus;
using System.Net;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service
{
    public class PatchOutcomesHttpTriggerService : IPatchOutcomesHttpTriggerService
    {
        private readonly IOutcomePatchService _outcomePatchService;
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly IOutcomesServiceBusClient _outcomesServiceBusClient;
        private readonly ILogger<PatchOutcomesHttpTriggerService> _logger;

        public PatchOutcomesHttpTriggerService(ICosmosDBProvider cosmosDbProvider,
            IOutcomePatchService outcomePatchService,
            IOutcomesServiceBusClient outcomesServiceBusClient,
            ILogger<PatchOutcomesHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _outcomePatchService = outcomePatchService;
            _outcomesServiceBusClient = outcomesServiceBusClient;
            _logger = logger;
        }

        public string UpdateOutcomeClaimedDateOutcomeEffectiveDateValue(string outcomeJson, bool setOutcomeClaimedDateToNull, bool setOutcomeEffectiveDateToNull)
        {
            if (string.IsNullOrEmpty(outcomeJson))
            {
                _logger.LogInformation("{outcomeJson} object is NULL or EMPTY.", nameof(outcomeJson));
                return null;
            }

            return _outcomePatchService.SetOutcomeClaimedDateOrOutcomeEffectiveDateToNull(outcomeJson, setOutcomeClaimedDateToNull, setOutcomeEffectiveDateToNull);
        }

        public string PatchResource(string outcomeJson, OutcomesPatch outcomesPatchPatch)
        {
            if (string.IsNullOrEmpty(outcomeJson))
                return null;

            if (outcomesPatchPatch == null)
                return null;

            outcomesPatchPatch.SetDefaultValues();

            return _outcomePatchService.Patch(outcomeJson, outcomesPatchPatch);
        }

        public async Task<Models.Outcomes> UpdateCosmosAsync(string outcomeJson, Guid outcomeId)
        {
            if (string.IsNullOrEmpty(outcomeJson))
                return null;

            var response = await _cosmosDbProvider.UpdateOutcomesAsync(outcomeJson, outcomeId);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? response.Resource : null;
        }

        public async Task<string> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionPlanId, Guid outcomeId)
        {
            var outcomes = await _cosmosDbProvider.GetOutcomesForCustomerAsyncToUpdateAsync(customerId, interactionsId, actionPlanId, outcomeId);

            return outcomes;
        }

        public async Task SendToServiceBusQueueAsync(Models.Outcomes outcomes, Guid customerId, string reqUrl)
        {
            await _outcomesServiceBusClient.SendPatchMessageAsync(outcomes, customerId, reqUrl);
        }
    }
}