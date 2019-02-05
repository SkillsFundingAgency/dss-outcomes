using System;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Models;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service
{
    public interface IPatchOutcomesHttpTriggerService
    {
        Task<Models.Outcomes> UpdateAsync(string outcomeJson, OutcomesPatch outcomesPatch, Guid outcomeId);
        Task<string> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionPlanId, Guid outcomeId);
        Task SendToServiceBusQueueAsync(Models.Outcomes outcomes, Guid customerId, string reqUrl);
    }
}