using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Service
{
    public class GetOutcomesHttpTriggerService : IGetOutcomesHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;

        public GetOutcomesHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<List<Models.Outcomes>> GetOutcomesAsync(Guid customerId)
        {
            var outcomes = await _documentDbProvider.GetOutcomesForCustomerAsync(customerId);

            return outcomes;
        }
    }
}