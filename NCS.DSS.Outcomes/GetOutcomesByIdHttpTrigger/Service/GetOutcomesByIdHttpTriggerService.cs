using System;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service
{
    public class GetOutcomesByIdHttpTriggerService : IGetOutcomesByIdHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;

        public GetOutcomesByIdHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }
        public async Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionId, Guid actionplanId, Guid outcomeId)
        {
            var outcomes = await _documentDbProvider.GetOutcomesForCustomerAsync(customerId, interactionId, actionplanId, outcomeId);

            return outcomes;
        }
    }
}