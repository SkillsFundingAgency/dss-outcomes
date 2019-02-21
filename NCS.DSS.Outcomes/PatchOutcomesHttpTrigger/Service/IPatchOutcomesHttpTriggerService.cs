using System;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Models;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service
{
    public interface IPatchOutcomesHttpTriggerService
    {
        string PatchResource(string outcomeJson, OutcomesPatch outcomesPatchPatch);
        Task<Models.Outcomes> UpdateCosmosAsync(string outcomeJson, Guid outcomeId);
        Task<string> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionPlanId, Guid outcomeId);
        Task SendToServiceBusQueueAsync(Models.Outcomes outcomes, Guid customerId, string reqUrl);
    }
}