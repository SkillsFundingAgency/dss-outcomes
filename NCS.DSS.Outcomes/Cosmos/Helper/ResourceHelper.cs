using System;
using NCS.DSS.Outcomes.Cosmos.Provider;

namespace NCS.DSS.Outcomes.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        public bool DoesCustomerExist(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesCustomerExist = documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public bool DoesInteractionExist(Guid interactionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesInteractionExist = documentDbProvider.DoesInteractionResourceExist(interactionId);

            return doesInteractionExist;
        }

        public bool DoesActionPlanExist(Guid actionplanId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesActionPlanExist = documentDbProvider.DoesActionPlanResourceExist(actionplanId);

            return doesActionPlanExist;
        }
    }
}
