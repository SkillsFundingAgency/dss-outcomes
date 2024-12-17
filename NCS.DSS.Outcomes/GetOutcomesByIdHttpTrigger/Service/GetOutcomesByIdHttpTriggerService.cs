using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service
{
    public class GetOutcomesByIdHttpTriggerService : IGetOutcomesByIdHttpTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly ILogger<GetOutcomesByIdHttpTriggerService> _logger;

        public GetOutcomesByIdHttpTriggerService(ICosmosDBProvider cosmosDbProvider, ILogger<GetOutcomesByIdHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _logger = logger;
        }
        public async Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionId, Guid actionplanId, Guid outcomeId)
        {
            _logger.LogInformation("Attempting to get Outcome for Customer. Customer ID: {CustomerId}.", customerId);
            var outcomes = await _cosmosDbProvider.GetOutcomeForCustomerAsync(customerId, interactionId, actionplanId, outcomeId);

            if (outcomes == null)
            {
                _logger.LogInformation("Outcome does not exist for Customer. Outcome GUID: {OutcomeId} Customer GUID: {CustomerId}", outcomeId, customerId);
                return null;
            }

            _logger.LogInformation("Outcome successfully retrieved. Outcome GUID: {OutcomeId} Customer GUID: {CustomerId}", outcomes.OutcomeId, customerId);
            return outcomes;
        }
    }
}