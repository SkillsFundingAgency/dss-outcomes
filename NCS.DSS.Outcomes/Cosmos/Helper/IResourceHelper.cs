using System;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
        bool IsCustomerReadOnly();
        int GetCustomerReasonForTermination();
        bool DoesActionPlanResourceExistAndBelongToCustomer(Guid actionplanId, Guid interactionId, Guid customerId);
        bool DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerId);
        Task<DateTime?> GetDateAndTimeOfSession(Guid sessionId);
    }
}