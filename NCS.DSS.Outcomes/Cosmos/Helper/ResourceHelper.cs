using System;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        private readonly IDocumentDBProvider _documentDbProvider;
        public ResourceHelper(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            var doesCustomerExist = await _documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            var isCustomerReadOnly = await _documentDbProvider.DoesCustomerHaveATerminationDate(customerId);

            return isCustomerReadOnly;
        }

        public bool DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            var doesInteractionExist = _documentDbProvider.DoesInteractionResourceExistAndBelongToCustomer(interactionId, customerId);

            return doesInteractionExist;
        }

        public bool DoesActionPlanResourceExistAndBelongToCustomer(Guid actionplanId, Guid interactionId, Guid customerId)
        {
            var doesActionPlanExist = _documentDbProvider.DoesActionPlanResourceExistAndBelongToCustomer(actionplanId, interactionId, customerId);

            return doesActionPlanExist;
        }

        public async Task<DateTime?> GetDateAndTimeOfSession(Guid sessionId)
        {
            var dateAndTimeOfSession = await _documentDbProvider.GetDateAndTimeOfSessionFromSessionResource(sessionId);

            return dateAndTimeOfSession;
        }
    }
}
