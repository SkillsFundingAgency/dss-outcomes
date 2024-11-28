using DFC.JSON.Standard;
using NCS.DSS.Outcomes.Cosmos.Provider;

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

        public bool DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            return _cosmosDbProvider.DoesInteractionResourceExistAndBelongToCustomer(interactionId, customerId);
        }

        public bool DoesActionPlanResourceExistAndBelongToCustomer(Guid actionplanId, Guid interactionId, Guid customerId)
        {
            return _cosmosDbProvider.DoesActionPlanResourceExistAndBelongToCustomer(actionplanId, interactionId, customerId);
        }

        public bool DoesSessionExistAndBelongToCustomer(Guid sessionId, Guid interactionId, Guid customerId)
        {
            return _cosmosDbProvider.DoesSessionResourceExistAndBelongToCustomer(sessionId, interactionId, customerId);
        }

        public async Task<DateTime?> GetDateAndTimeOfSession(Guid sessionId)
        {
            var dateAndTimeOfSession = await _cosmosDbProvider.GetDateAndTimeOfSessionFromSessionResource(sessionId);

            return dateAndTimeOfSession;
        }

    }
}
