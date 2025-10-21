using NCS.DSS.Outcomes.ReferenceData;

namespace NCS.DSS.Outcomes.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
        bool IsCustomerReadOnly();
        int GetCustomerReasonForTermination();
        Task<bool> DoesActionPlanResourceExistAndBelongToCustomer(Guid actionplanId, Guid interactionId, Guid customerId);
        Task<bool> DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerId);
        Task<bool> DoesSessionExistAndBelongToCustomer(Guid sessionId, Guid interactionId, Guid customerId);
        Task<bool> DoesSessionExistAndBelongToCustomerActionPlan(Guid sessionId, Guid interactionId, Guid actionPlanId,Guid customerId);
        Task<bool> DoesClaimedOutcomeExistForCustomerAsync(Guid customerId, Guid sessionId, Guid actionPlanId, OutcomeType outcomeType);
        Task<bool> DoesClaimedOutcomeExistForCustomerAsync(Guid customerId, Guid actionPlanId, string outcomeJson, string outcomeType);
        Task<DateTime?> GetDateAndTimeOfSession(Guid sessionId);

    }
}
