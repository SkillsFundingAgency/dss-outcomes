﻿using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.ServiceBus;
using System.Net;

namespace NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service
{
    public class PostOutcomesHttpTriggerService : IPostOutcomesHttpTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly IOutcomesServiceBusClient _outcomesServiceBusClient;

        public PostOutcomesHttpTriggerService(ICosmosDBProvider cosmosDbProvider, IOutcomesServiceBusClient outcomesServiceBusClient)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _outcomesServiceBusClient = outcomesServiceBusClient;
        }

        public async Task<Models.Outcomes> CreateAsync(Models.Outcomes outcomes)
        {
            if (outcomes == null)
                return null;

            outcomes.SetDefaultValues();

            var response = await _cosmosDbProvider.CreateOutcomesAsync(outcomes);

            return response.StatusCode == HttpStatusCode.Created ? response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Outcomes outcomes, string reqUrl)
        {
            await _outcomesServiceBusClient.SendPostMessageAsync(outcomes, reqUrl);
        }
    }
}