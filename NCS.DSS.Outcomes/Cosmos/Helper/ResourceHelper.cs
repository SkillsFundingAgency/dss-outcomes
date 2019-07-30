using System;
using System.Threading.Tasks;
using DFC.JSON.Standard;
using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly IJsonHelper _jsonHelper;

        public ResourceHelper(IDocumentDBProvider documentDbProvider, IJsonHelper jsonHelper)
        {
            _documentDbProvider = documentDbProvider;
            _jsonHelper = jsonHelper;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _documentDbProvider.DoesCustomerResourceExist(customerId);
        }

        public bool IsCustomerReadOnly()
        {
            var customerJson = _documentDbProvider.GetCustomerJson();
            
            if (string.IsNullOrWhiteSpace(customerJson))
                return false;
            
            var dateOfTermination = _jsonHelper.GetValue(customerJson, "DateOfTermination");

            return !string.IsNullOrWhiteSpace(dateOfTermination);
        }

        public int GetCustomerReasonForTermination()
        {
            var customerJson = _documentDbProvider.GetCustomerJson();
            
            if (string.IsNullOrWhiteSpace(customerJson))
                return 99;
            
            var reasonForTermination = _jsonHelper.GetValue(customerJson, "ReasonForTermination");

            return string.IsNullOrWhiteSpace(reasonForTermination) ? 99 : int.Parse(reasonForTermination);
        }

        public bool DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            return _documentDbProvider.DoesInteractionResourceExistAndBelongToCustomer(interactionId, customerId); 
        }

        public bool DoesActionPlanResourceExistAndBelongToCustomer(Guid actionplanId, Guid interactionId, Guid customerId)
        {
            return _documentDbProvider.DoesActionPlanResourceExistAndBelongToCustomer(actionplanId, interactionId, customerId);
        }

        public bool DoesSessionExistAndBelongToCustomer(Guid sessionId, Guid interactionId, Guid customerId)
        {
            return _documentDbProvider.DoesSessionResourceExistAndBelongToCustomer(sessionId, interactionId, customerId);
        }

        public DateTime? GetDateAndTimeOfSession(Guid sessionId)
        {
            var sessionForCustomerJson = _documentDbProvider.GetSessionForCustomerJson();

            if (string.IsNullOrWhiteSpace(sessionForCustomerJson))
                return null;

            var dateAndTimeOfSession = _jsonHelper.GetValue(sessionForCustomerJson, "DateandTimeOfSession");

            if (string.IsNullOrEmpty(dateAndTimeOfSession))
                return null;

            return DateTime.Parse(dateAndTimeOfSession);
        }

    }
}
