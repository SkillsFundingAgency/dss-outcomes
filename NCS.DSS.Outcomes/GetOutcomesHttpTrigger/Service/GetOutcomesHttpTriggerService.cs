using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Service
{
    public class GetOutcomesHttpTriggerService : IGetOutcomesHttpTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;

        public GetOutcomesHttpTriggerService(ICosmosDBProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }

        public async Task<List<Models.Outcomes>> GetOutcomesAsync(Guid customerId)
        {
            var outcomes = await _cosmosDbProvider.GetOutcomesForCustomerAsync(customerId);

            return outcomes;
        }
    }
}