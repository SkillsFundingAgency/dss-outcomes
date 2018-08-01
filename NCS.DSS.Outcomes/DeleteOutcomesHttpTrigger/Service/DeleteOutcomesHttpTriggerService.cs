using System;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.DeleteOutcomesHttpTrigger.Service
{
    public class DeleteOutcomesHttpTriggerService : IDeleteOutcomesHttpTriggerService
    {
        public async Task<bool> DeleteAsync(Guid outcomesId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var outcomes = await documentDbProvider.DeleteAsync(outcomesId);

            return outcomes;
        }

        public async Task<Models.Outcomes> GetOutcomeForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid outcomesId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var outcome = await documentDbProvider.GetOutcomesForCustomerAsync(customerId, interactionsId, actionplanId, outcomesId);

            return outcome;
        }

    }
}