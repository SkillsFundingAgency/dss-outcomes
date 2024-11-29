using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace NCS.DSS.Outcomes.Cosmos.Provider
{
    public class CosmosDBProvider : ICosmosDBProvider
    {
        private string _customerJson;
        private readonly Container _container;
        private readonly Container _customerContainer;
        private readonly Container _sessionContainer;
        private readonly Container _interactionContainer;
        private readonly Container _actionPlanContainer;

        private readonly string _databaseId = Environment.GetEnvironmentVariable("DatabaseId");
        private readonly string _containerId = Environment.GetEnvironmentVariable("CollectionId");
        private readonly string _customerDatabaseId = Environment.GetEnvironmentVariable("CustomerDatabaseId");
        private readonly string _customerContainerId = Environment.GetEnvironmentVariable("CustomerCollectionId");
        private readonly string _sessionDatabaseId = Environment.GetEnvironmentVariable("SessionDatabaseId");
        private readonly string _sessionContainerId = Environment.GetEnvironmentVariable("SessionCollectionId");
        private readonly string _interactionDatabaseId = Environment.GetEnvironmentVariable("InteractionDatabaseId");
        private readonly string _interactionContainerId = Environment.GetEnvironmentVariable("InteractionCollectionId");
        private readonly string _actionPlanDatabaseId = Environment.GetEnvironmentVariable("ActionPlanDatabaseId");
        private readonly string _actionPlanContainerId = Environment.GetEnvironmentVariable("ActionPlanCollectionId");

        public CosmosDBProvider(CosmosClient cosmosClient)
        {
            _container = cosmosClient.GetContainer(_databaseId, _containerId);
            _customerContainer = cosmosClient.GetContainer(_customerDatabaseId, _customerContainerId);
            _sessionContainer = cosmosClient.GetContainer(_sessionDatabaseId, _sessionContainerId);
            _interactionContainer = cosmosClient.GetContainer(_interactionDatabaseId, _interactionContainerId);
            _actionPlanContainer = cosmosClient.GetContainer(_actionPlanDatabaseId, _actionPlanContainerId);
        }

        public string GetCustomerJson()
        {
            return _customerJson;
        }

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            string queryText = "SELECT TOP 1 * FROM c WHERE c.id = @customerId";
            var queryDefinition = new QueryDefinition(queryText)
                .WithParameter("@customerId", customerId.ToString());

            using var iterator = _customerContainer.GetItemQueryIterator<dynamic>(queryDefinition);

            var response = await iterator.ReadNextAsync();

            var customerData = response.FirstOrDefault();
            if (customerData != null)
            {
                _customerJson = customerData.ToString();
                return true;
            }

            return false;
        }

        public async Task<DateTime?> GetDateAndTimeOfSessionFromSessionResource(Guid sessionId)
        {
            try
            {
                string queryText = "SELECT TOP 1 * FROM c WHERE c.id = @sessionId";
                var queryDefinition = new QueryDefinition(queryText)
                    .WithParameter("@sessionId", sessionId.ToString());

                using var iterator = _sessionContainer.GetItemQueryIterator<dynamic>(queryDefinition);

                var response = await iterator.ReadNextAsync();

                return GetDateAndTimeOfSessionFromResponse(response);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private DateTime? GetDateAndTimeOfSessionFromResponse(FeedResponse<dynamic> response)
        {
            return response
                .Select(item => (DateTime?)item["DateandTimeOfSession"])
                .FirstOrDefault();
        }

        public async Task<bool> DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            try
            {
                string queryText = "SELECT VALUE COUNT(1) FROM interactions i WHERE i.id = @interactionId AND i.CustomerId = @customerId";
                var queryDefinition = new QueryDefinition(queryText)
                    .WithParameter("@interactionId", interactionId.ToString())
                    .WithParameter("@customerId", customerId.ToString());

                using var iterator = _interactionContainer.GetItemQueryIterator<dynamic>(queryDefinition);

                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    return response.FirstOrDefault() > 0;
                }

                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task<bool> DoesSessionResourceExistAndBelongToCustomer(Guid sessionId, Guid interactionId, Guid customerId)
        {
            try
            {
                string queryText = "SELECT VALUE COUNT(1) FROM sessions s WHERE s.id = @sessionId AND s.InteractionId = @interactionId AND s.CustomerId = @customerId";
                var queryDefinition = new QueryDefinition(queryText)
                    .WithParameter("@sessionId", sessionId.ToString())
                    .WithParameter("@interactionId", interactionId.ToString())
                    .WithParameter("@customerId", customerId.ToString());

                using var iterator = _sessionContainer.GetItemQueryIterator<int>(queryDefinition);

                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    return response.FirstOrDefault() > 0;
                }

                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task<bool> DoesActionPlanResourceExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId)
        {
            try
            {
                string queryText = "SELECT VALUE COUNT(1) FROM actionplans a WHERE a.id = @actionPlanId AND a.InteractionId = @interactionId AND a.CustomerId = @customerId";
                var queryDefinition = new QueryDefinition(queryText)
                    .WithParameter("@actionPlanId", actionPlanId.ToString())
                    .WithParameter("@interactionId", interactionId.ToString())
                    .WithParameter("@customerId", customerId.ToString());

                using var iterator = _actionPlanContainer.GetItemQueryIterator<dynamic>(queryDefinition);

                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    return response.FirstOrDefault() > 0;
                }

                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task<List<Models.Outcomes>> GetOutcomesForCustomerAsync(Guid customerId)
        {
            string queryText = "SELECT * FROM c Where c.CustomerId = @customerId";
            QueryDefinition queryDefinition = new QueryDefinition(queryText)
                .WithParameter("@customerId", customerId.ToString());

            var outcomes = new List<Models.Outcomes>();

            using (FeedIterator<Models.Outcomes> iterator = _container.GetItemQueryIterator<Models.Outcomes>(queryDefinition))
            {
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    outcomes.AddRange(response);
                }
            }

            return outcomes.Any() ? outcomes : null;
        }

        public async Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionPlanId, Guid outcomeId)
        {
            string queryText = "SELECT * FROM c Where c.CustomerId = @customerId AND c.ActionPlanId = @actionPlanId AND c.id = @outcomeId";
            QueryDefinition queryDefinition = new QueryDefinition(queryText)
                .WithParameter("@customerId", customerId.ToString())
                .WithParameter("@actionPlanId", actionPlanId.ToString())
                .WithParameter("@outcomeId", outcomeId.ToString());

            var outcomes = new List<Models.Outcomes>();

            using (FeedIterator<Models.Outcomes> iterator = _container.GetItemQueryIterator<Models.Outcomes>(queryDefinition))
            {
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    outcomes.AddRange(response);
                }
            }

            return outcomes.Any() ? outcomes.FirstOrDefault() : null;
        }

        public async Task<string> GetOutcomesForCustomerAsyncToUpdateAsync(Guid customerId, Guid interactionsId, Guid actionPlanId, Guid outcomeId)
        {
            var outcomes = await GetOutcomesForCustomerAsync(customerId, interactionsId, actionPlanId, outcomeId);

            return JsonConvert.SerializeObject(outcomes);
        }

        public async Task<ItemResponse<Models.Outcomes>> CreateOutcomesAsync(Models.Outcomes outcome)
        {
            ItemResponse<Models.Outcomes> response = await _container.CreateItemAsync(outcome);

            return response;
        }

        public async Task<ItemResponse<Models.Outcomes>> UpdateOutcomesAsync(string outcomeJson, Guid outcomeId)
        {
            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(outcomeJson);

            var response = await _container.ReplaceItemAsync(outcome, outcomeId.ToString());
            return response;
        }
    }
}