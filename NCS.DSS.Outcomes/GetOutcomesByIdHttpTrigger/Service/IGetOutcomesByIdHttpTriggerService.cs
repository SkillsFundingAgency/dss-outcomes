using System;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service
{
    public interface IGetOutcomesByIdHttpTriggerService
    {
        Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid OutcomesId);
    }
}