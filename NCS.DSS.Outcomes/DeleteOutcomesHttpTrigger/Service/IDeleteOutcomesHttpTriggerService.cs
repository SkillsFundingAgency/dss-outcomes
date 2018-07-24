using System;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.DeleteOutcomesHttpTrigger.Service
{
    public interface IDeleteOutcomesHttpTriggerService
    {
        Task<bool> DeleteAsync(Guid outcomesId);
        Task<Models.Outcomes> GetOutcomeForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid outcomesId);
    }
}