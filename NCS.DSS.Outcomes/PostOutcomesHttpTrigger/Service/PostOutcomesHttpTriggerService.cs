using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service
{
    public class PostOutcomesHttpTriggerService : IPostOutcomesHttpTriggerService
    {
        public async Task<Models.Outcomes> CreateAsync(Models.Outcomes Outcomes)
        {
            if (Outcomes == null)
                return null;

            Outcomes.SetDefaultValues();

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateOutcomesAsync(Outcomes);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }
    }
}