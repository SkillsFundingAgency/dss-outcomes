using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Service
{
    public class GetOutcomesHttpTriggerService : IGetOutcomesHttpTriggerService
    {
        public async Task<List<Models.Outcomes>> GetOutcomesAsync(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var Outcomes = await documentDbProvider.GetOutcomesForCustomerAsync(customerId);

            return Outcomes;
        }
    }
}