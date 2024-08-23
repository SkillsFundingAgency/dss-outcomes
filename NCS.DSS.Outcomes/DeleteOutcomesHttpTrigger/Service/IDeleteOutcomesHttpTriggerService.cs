namespace NCS.DSS.Outcomes.DeleteOutcomesHttpTrigger.Service
{
    public interface IDeleteOutcomesHttpTriggerService
    {
        Task<bool> DeleteAsync(Guid OutcomeId);
        Task<Models.Outcomes> GetOutcomeForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid OutcomeId);
    }
}