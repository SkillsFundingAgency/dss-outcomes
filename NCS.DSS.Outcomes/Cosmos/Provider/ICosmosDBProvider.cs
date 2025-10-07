using Microsoft.Azure.Cosmos;
using NCS.DSS.Outcomes.ReferenceData;

namespace NCS.DSS.Outcomes.Cosmos.Provider
{
    public interface ICosmosDBProvider
    {
        string GetCustomerJson();
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId);
        Task<bool> DoesSessionResourceExistAndBelongToCustomer(Guid sessionId, Guid interactionId, Guid customerId);
        Task<bool> DoesActionPlanResourceExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId);
        Task<bool> DoesOutcomeExistForCustomerAsync(Guid customerId, Guid sessionId, Guid actionPlanId, OutcomeType outcomeType);

        Task<List<Models.Outcomes>> GetOutcomesForCustomerAsync(Guid customerId);
        Task<string> GetOutcomesForCustomerAsyncToUpdateAsync(Guid customerId, Guid actionPlanId, Guid outcomeId);
        Task<Models.Outcomes> GetOutcomeForCustomerAsync(Guid customerId, Guid actionplanId, Guid outcomeId);
        Task<ItemResponse<Models.Outcomes>> CreateOutcomesAsync(Models.Outcomes outcomes);
        Task<ItemResponse<Models.Outcomes>> UpdateOutcomesAsync(string outcomeJson, Guid outcomeId);
        Task<DateTime?> GetDateAndTimeOfSessionFromSessionResource(Guid sessionId);
    }
}