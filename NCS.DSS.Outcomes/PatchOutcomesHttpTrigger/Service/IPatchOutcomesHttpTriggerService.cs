using System;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service
{
    public interface IPatchOutcomesHttpTriggerService
    {
        Task<Models.Outcomes> UpdateAsync(Models.Outcomes Outcomes, Models.OutcomesPatch outcomesPatch);
        Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid outcomesId);
    }
}