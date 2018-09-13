using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.Models;
using NCS.DSS.Outcomes.ServiceBus;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service
{
    public class PatchOutcomesHttpTriggerService : IPatchOutcomesHttpTriggerService
    {
        public async Task<Models.Outcomes> UpdateAsync(Models.Outcomes outcomes, OutcomesPatch outcomesPatch)
        {
            if (outcomes == null)
                return null;

            outcomesPatch.SetDefaultValues();

            outcomes.Patch(outcomesPatch);

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateOutcomesAsync(outcomes);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? outcomes : null;
        }

        public async Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid OutcomeId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var outcomes = await documentDbProvider.GetOutcomesForCustomerAsync(customerId, interactionsId, actionplanId, OutcomeId);

            return outcomes;
        }

        public async Task SendToServiceBusQueueAsync(Models.Outcomes outcomes, Guid customerId, string reqUrl)
        {
            await ServiceBusClient.SendPatchMessageAsync(outcomes, customerId, reqUrl);
        }
    }
}