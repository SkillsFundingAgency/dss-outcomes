using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.Models;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service
{
    public class PatchOutcomesHttpTriggerService : IPatchOutcomesHttpTriggerService
    {
        public async Task<Models.Outcomes> UpdateAsync(Models.Outcomes Outcomes, OutcomesPatch OutcomesPatch)
        {
            if (Outcomes == null)
                return null;

            if (!OutcomesPatch.LastModifiedDate.HasValue)
                OutcomesPatch.LastModifiedDate = DateTime.Now;

            Outcomes.Patch(OutcomesPatch);

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateOutcomesAsync(Outcomes);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? Outcomes : null;
        }

        public async Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid OutcomesId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var Outcomes = await documentDbProvider.GetOutcomesForCustomerAsync(customerId, interactionsId, actionplanId, OutcomesId);

            return Outcomes;
        }
    }
}