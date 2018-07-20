using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Outcomes.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        bool DoesCustomerResourceExist(Guid customerId);
        bool DoesInteractionResourceExist(Guid interactionId);
        Task<List<Models.Outcomes>> GetOutcomesForCustomerAsync(Guid customerId);
        Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid OutcomesId);
        Task<ResourceResponse<Document>> CreateOutcomesAsync(Models.Outcomes Outcomes);
        Task<ResourceResponse<Document>> UpdateOutcomesAsync(Models.Outcomes Outcomes);
    }
}