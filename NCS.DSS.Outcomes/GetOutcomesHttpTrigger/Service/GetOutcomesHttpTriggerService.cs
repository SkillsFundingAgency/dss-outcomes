using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Service
{
    public class GetOutcomesHttpTriggerService : IGetOutcomesHttpTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly ILogger<GetOutcomesHttpTriggerService> _logger;

        public GetOutcomesHttpTriggerService(ICosmosDBProvider cosmosDbProvider, ILogger<GetOutcomesHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _logger = logger;
        }

        public async Task<List<Models.Outcomes>> GetOutcomesAsync(Guid customerId)
        {
            _logger.LogInformation("Attempting to get Outcomes for Customer. Customer ID: {CustomerId}.", customerId);
            var outcomes = await _cosmosDbProvider.GetOutcomesForCustomerAsync(customerId);

            if (outcomes == null)
            {
                _logger.LogInformation("Outcome does not exist for Customer. Customer GUID: {CustomerId}", customerId);
                return null;
            }

            _logger.LogInformation("{Count} Outcomes successfully retrieved. Customer GUID: {CustomerId}", outcomes.Count, customerId);
            return outcomes;
        }
    }
}