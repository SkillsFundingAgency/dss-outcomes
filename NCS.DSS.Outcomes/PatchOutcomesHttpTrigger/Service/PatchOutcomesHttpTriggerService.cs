using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.Models;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service
{
    public class PatchOutcomesHttpTriggerService : IPatchOutcomesHttpTriggerService
    {
        public async Task<Models.Outcomes> UpdateAsync(Models.Outcomes outcomes, OutcomesPatch outcomesPatch)
        {
            if (outcomes == null)
                return null;

            if (!outcomesPatch.LastModifiedDate.HasValue)
                outcomesPatch.LastModifiedDate = DateTime.Now;

            outcomes.Patch(outcomesPatch);

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateOutcomesAsync(outcomes);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? outcomes : null;
        }

        public async Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid outcomesId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var outcomes = await documentDbProvider.GetOutcomesForCustomerAsync(customerId, interactionsId, actionplanId, outcomesId);

            return outcomes;
        }
    }
}