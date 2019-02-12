using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.ServiceBus;

namespace NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service
{
    public class PostOutcomesHttpTriggerService : IPostOutcomesHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;

        public PostOutcomesHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.Outcomes> CreateAsync(Models.Outcomes outcomes)
        {
            if (outcomes == null)
                return null;

            outcomes.SetDefaultValues();

            var response = await _documentDbProvider.CreateOutcomesAsync(outcomes);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Outcomes outcomes, string reqUrl)
        {
            await ServiceBusClient.SendPostMessageAsync(outcomes, reqUrl);
        }
    }
}