using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Outcomes.Models;
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
        private readonly ILogger<CosmosDBProvider> _logger;

        public CosmosDBProvider(CosmosClient cosmosClient,
            IOptions<OutcomesConfigurationSettings> configOptions,
            ILogger<CosmosDBProvider> logger)
        {
            var config = configOptions.Value;

            _container = GetContainer(cosmosClient, config.DatabaseId, config.CollectionId);
            _customerContainer = GetContainer(cosmosClient, config.CustomerDatabaseId, config.CustomerCollectionId);
            _sessionContainer = GetContainer(cosmosClient, config.SessionDatabaseId, config.SessionCollectionId);
            _interactionContainer = GetContainer(cosmosClient, config.InteractionDatabaseId, config.InteractionCollectionId);
            _actionPlanContainer = GetContainer(cosmosClient, config.ActionPlanDatabaseId, config.ActionPlanCollectionId);
            _logger = logger;
        }

        private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId)
            => cosmosClient.GetContainer(databaseId, collectionId);

        public string GetCustomerJson()
        {
            return _customerJson;
        }

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            try
            {
                _logger.LogInformation("Checking for customer resource. Customer ID: {CustomerId}", customerId);

                string queryText = "SELECT TOP 1 * FROM c WHERE c.id = @customerId";
                var queryDefinition = new QueryDefinition(queryText)
                    .WithParameter("@customerId", customerId.ToString());

                using var iterator = _customerContainer.GetItemQueryIterator<dynamic>(queryDefinition);

                var response = await iterator.ReadNextAsync();

                var customerData = response.FirstOrDefault();
                if (customerData != null)
                {
                    _customerJson = customerData.ToString();
                    _logger.LogInformation("Customer exists. Customer ID: {CustomerId}", customerId);
                    return true;
                }
                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking customer resource existence. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<DateTime?> GetDateAndTimeOfSessionFromSessionResource(Guid sessionId)
        {
            _logger.LogInformation("Attempting to retrieve DateAndTimeOfSession. Session ID: {SessionId}", sessionId);
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
                _logger.LogInformation("Session does not exist. Session ID: {SessionId}", sessionId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DateAndTimeOfSession. Session ID: {SessionId}", sessionId);
                throw;
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
                _logger.LogInformation("Checking for interaction resource for a customer. Customer ID: {CustomerId} Interaction ID: {InteractionId}", customerId, interactionId);

                string queryText = "SELECT VALUE COUNT(1) FROM interactions i WHERE i.id = @interactionId AND i.CustomerId = @customerId";
                var queryDefinition = new QueryDefinition(queryText)
                    .WithParameter("@interactionId", interactionId.ToString())
                    .WithParameter("@customerId", customerId.ToString());

                using var iterator = _interactionContainer.GetItemQueryIterator<dynamic>(queryDefinition);

                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    var interactionFound = response.FirstOrDefault() > 0;

                    if (interactionFound)
                    {
                        _logger.LogInformation("Interaction for customer exists. Customer ID: {CustomerId} Interaction ID: {InteractionId}", customerId, interactionId);
                    }
                    return interactionFound;
                }

                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Interaction for customer is not found. Customer ID: {CustomerId} Interaction ID: {InteractionId}", customerId, interactionId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking interaction resource for a customer. Customer ID: {CustomerId} Interaction ID: {InteractionId}", customerId, interactionId);
                throw;
            }
        }

        public async Task<bool> DoesSessionResourceExistAndBelongToCustomer(Guid sessionId, Guid interactionId, Guid customerId)
        {
            _logger.LogInformation("Checking for session resource for a customer. Customer ID: {CustomerId} Interaction ID: {InteractionId} Session ID: {SessionId}", customerId, interactionId, sessionId);
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
                    var sessionExists = response.FirstOrDefault() > 0;
                    if (sessionExists)
                    {
                        _logger.LogInformation("Session for customer exists. Customer ID: {CustomerId} Interaction ID: {InteractionId} Session ID: {SessionId}", customerId, interactionId, sessionId);
                    }
                    return sessionExists;
                }

                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Session for customer is not found. Customer ID: {CustomerId} Interaction ID: {InteractionId} Session ID: {SessionId}", customerId, interactionId, sessionId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking session resource for a customer. Customer ID: {CustomerId} Interaction ID: {InteractionId} Session ID: {SessionId}", customerId, interactionId, sessionId);
                throw;
            }
        }

        public async Task<bool> DoesActionPlanResourceExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId)
        {
            _logger.LogInformation("Checking for action plan resource for a customer. Customer ID: {CustomerId} Interaction ID: {InteractionId} ActionPlan ID: {ActionPlanId}", customerId, interactionId, actionPlanId);
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
                    var actionPlanExists = response.FirstOrDefault() > 0;
                    if (actionPlanExists)
                    {
                        _logger.LogInformation("Action plan for customer exists. Customer ID: {CustomerId} Interaction ID: {InteractionId} ActionPlan ID: {ActionPlanId}", customerId, interactionId, actionPlanId);
                    }
                    return actionPlanExists;
                }

                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Action plan for customer is not found. Customer ID: {CustomerId} Interaction ID: {InteractionId} ActionPlan ID: {ActionPlanId}", customerId, interactionId, actionPlanId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking action plan resource for a customer. Customer ID: {CustomerId} Interaction ID: {InteractionId} ActionPlan ID: {ActionPlanId}", customerId, interactionId, actionPlanId);
                throw;
            }
        }

        public async Task<List<Models.Outcomes>> GetOutcomesForCustomerAsync(Guid customerId)
        {
            _logger.LogInformation("Attempting to retrieve Outcomes for a Customer. Customer ID: {CustomerId}", customerId);

            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Outcomes for a Customer. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<Models.Outcomes> GetOutcomeForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionPlanId, Guid outcomeId)
        {
            _logger.LogInformation("Attempting to retrieve Outcome for a Customer. Customer ID: {CustomerId} Interaction ID: {InteractionId} ActionPlan ID: {ActionPlanId} OutcomeId ID: {OutcomeId}", customerId, interactionsId, actionPlanId, outcomeId);

            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Outcome for a Customer. Customer ID: {CustomerId} Interaction ID: {InteractionId} ActionPlan ID: {ActionPlanId} OutcomeId ID: {OutcomeId}", customerId, interactionsId, actionPlanId, outcomeId);
                throw;
            }
        }

        public async Task<string> GetOutcomesForCustomerAsyncToUpdateAsync(Guid customerId, Guid interactionsId, Guid actionPlanId, Guid outcomeId)
        {
            var outcome = await GetOutcomeForCustomerAsync(customerId, interactionsId, actionPlanId, outcomeId);

            return JsonConvert.SerializeObject(outcome);
        }

        public async Task<ItemResponse<Models.Outcomes>> CreateOutcomesAsync(Models.Outcomes outcome)
        {
            _logger.LogInformation("Creating Outcome. Outcome ID: {OutcomeId}", outcome.OutcomeId);

            ItemResponse<Models.Outcomes> response = await _container.CreateItemAsync(outcome);

            _logger.LogInformation("Finished creating Outcome. Outcome ID: {OutcomeID}", outcome.OutcomeId);

            return response;
        }

        public async Task<ItemResponse<Models.Outcomes>> UpdateOutcomesAsync(string outcomeJson, Guid outcomeId)
        {
            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(outcomeJson);

            _logger.LogInformation("Updating Outcome. Outcome ID: {OutcomeId}", outcomeId);

            var response = await _container.ReplaceItemAsync(outcome, outcomeId.ToString());

            _logger.LogInformation("Finished updating Outcome. Outcome ID: {OutcomeID}", outcomeId);

            return response;
        }
    }
}