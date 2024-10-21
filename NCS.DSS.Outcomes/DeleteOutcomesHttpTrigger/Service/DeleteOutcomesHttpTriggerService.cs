using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.DeleteOutcomesHttpTrigger.Service
{
    public class DeleteOutcomesHttpTriggerService : IDeleteOutcomesHttpTriggerService
    {
        public async Task<bool> DeleteAsync(Guid OutcomeId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var outcomes = await documentDbProvider.DeleteAsync(OutcomeId);

            return outcomes;
        }

        public async Task<Models.Outcomes> GetOutcomeForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid OutcomeId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var outcome = await documentDbProvider.GetOutcomesForCustomerAsync(customerId, interactionsId, actionplanId, OutcomeId);

            return outcome;
        }

    }
}