using DFC.JSON.Standard;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.ReferenceData;

namespace NCS.DSS.Outcomes.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly IJsonHelper _jsonHelper;

        public ResourceHelper(ICosmosDBProvider documentDbProvider, IJsonHelper jsonHelper)
        {
            _cosmosDbProvider = documentDbProvider;
            _jsonHelper = jsonHelper;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _cosmosDbProvider.DoesCustomerResourceExist(customerId);
        }

        public bool IsCustomerReadOnly()
        {
            var customerJson = _cosmosDbProvider.GetCustomerJson();

            if (string.IsNullOrWhiteSpace(customerJson))
                return false;

            var dateOfTermination = _jsonHelper.GetValue(customerJson, "DateOfTermination");

            return !string.IsNullOrWhiteSpace(dateOfTermination);
        }

        public int GetCustomerReasonForTermination()
        {
            var customerJson = _cosmosDbProvider.GetCustomerJson();

            if (string.IsNullOrWhiteSpace(customerJson))
                return 99;

            var reasonForTermination = _jsonHelper.GetValue(customerJson, "ReasonForTermination");

            return string.IsNullOrWhiteSpace(reasonForTermination) ? 99 : int.Parse(reasonForTermination);
        }

        public async Task<bool> DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            return await _cosmosDbProvider.DoesInteractionResourceExistAndBelongToCustomer(interactionId, customerId);
        }

        public async Task<bool> DoesActionPlanResourceExistAndBelongToCustomer(Guid actionplanId, Guid interactionId, Guid customerId)
        {
            return await _cosmosDbProvider.DoesActionPlanResourceExistAndBelongToCustomer(actionplanId, interactionId, customerId);
        }

        public async Task<bool> DoesClaimedOutcomeExistForCustomerAsync(Guid customerId, Guid sessionId, Guid actionPlanId, OutcomeType outcomeType)
        {
            return await _cosmosDbProvider.DoesClaimedOutcomeExistForCustomerAsync(customerId, sessionId, actionPlanId, outcomeType);
        }

        public async Task<bool> DoesClaimedOutcomeExistForCustomerAsync(Guid customerId, Guid actionPlanId, string outcomeJson, string outcomeType)
        {
            return await _cosmosDbProvider.DoesClaimedOutcomeExistForCustomerAsync(customerId, actionPlanId, outcomeJson, outcomeType);
        }


        public async Task<bool> DoesSessionExistAndBelongToCustomer(Guid sessionId, Guid interactionId, Guid customerId)
        {
            return await _cosmosDbProvider.DoesSessionResourceExistAndBelongToCustomer(sessionId, interactionId, customerId);
        }

        public async Task<bool> DoesSessionExistAndBelongToCustomerActionPlan(Guid sessionId, Guid interactionId, Guid actionPlanId, Guid customerId)
        {
            return await _cosmosDbProvider.DoesSessionExistAndBelongToCustomerActionPlan(sessionId, interactionId, actionPlanId,customerId);
        }


        public async Task<DateTime?> GetDateAndTimeOfSession(Guid sessionId)
        {
            var dateAndTimeOfSession = await _cosmosDbProvider.GetDateAndTimeOfSessionFromSessionResource(sessionId);

            return dateAndTimeOfSession;
        }
    }
}
