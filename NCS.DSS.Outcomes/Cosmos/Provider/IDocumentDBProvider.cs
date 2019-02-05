using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Outcomes.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        bool DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId);
        bool DoesActionPlanResourceExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        Task<List<Models.Outcomes>> GetOutcomesForCustomerAsync(Guid customerId);
        Task<string> GetOutcomesForCustomerAsyncToUpdateAsync(Guid customerId, Guid interactionsId, Guid actionPlanId, Guid outcomeId);

        Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid outcomeId);
        Task<ResourceResponse<Document>> CreateOutcomesAsync(Models.Outcomes outcomes);
        Task<ResourceResponse<Document>> UpdateOutcomesAsync(string outcomesJson, Guid outcomeId);
        Task<bool> DeleteAsync(Guid outcomeId);
    }
}