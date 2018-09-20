using System;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesCustomerExist = await documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var isCustomerReadOnly = await documentDbProvider.DoesCustomerHaveATerminationDate(customerId);

            return isCustomerReadOnly;
        }

        public async Task<bool> DoesInteractionExist(Guid interactionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesInteractionExist = await documentDbProvider.DoesInteractionResourceExist(interactionId);

            return doesInteractionExist;
        }

        public async Task<bool> DoesActionPlanExist(Guid actionplanId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesActionPlanExist = await documentDbProvider.DoesActionPlanResourceExist(actionplanId);

            return doesActionPlanExist;
        }
    }
}
