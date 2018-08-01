using System;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service
{
    public class GetOutcomesByIdHttpTriggerService : IGetOutcomesByIdHttpTriggerService
    {
        public async Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionId, Guid actionplanId, Guid OutcomesId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var Outcomes = await documentDbProvider.GetOutcomesForCustomerAsync(customerId, interactionId, actionplanId, OutcomesId);

            return Outcomes;
        }
    }
}