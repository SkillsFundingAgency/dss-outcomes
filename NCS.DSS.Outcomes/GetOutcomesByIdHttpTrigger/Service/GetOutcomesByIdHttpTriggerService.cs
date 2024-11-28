using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service
{
    public class GetOutcomesByIdHttpTriggerService : IGetOutcomesByIdHttpTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;

        public GetOutcomesByIdHttpTriggerService(ICosmosDBProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }
        public async Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionId, Guid actionplanId, Guid outcomeId)
        {
            var outcomes = await _cosmosDbProvider.GetOutcomesForCustomerAsync(customerId, interactionId, actionplanId, outcomeId);

            return outcomes;
        }
    }
}