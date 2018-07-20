using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Outcomes.Cosmos.Client;
using NCS.DSS.Outcomes.Cosmos.Helper;

namespace NCS.DSS.Outcomes.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly DocumentDBHelper _documentDbHelper;
        private readonly DocumentDBClient _databaseClient;

        public DocumentDBProvider()
        {
            _documentDbHelper = new DocumentDBHelper();
            _databaseClient = new DocumentDBClient();
        }

        public bool DoesCustomerResourceExist(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateCustomerDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var customerQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return customerQuery.Where(x => x.Id == customerId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public bool DoesInteractionResourceExist(Guid interactionId)
        {
            var collectionUri = _documentDbHelper.CreateInteractionDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var interactionQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return interactionQuery.Where(x => x.Id == interactionId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public bool DoesActionPlanResourceExist(Guid actionplanId)
        {
            var collectionUri = _documentDbHelper.CreateActionPlanDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var actionplanQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return actionplanQuery.Where(x => x.Id == actionplanId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public async Task<List<Models.Outcomes>> GetOutcomesForCustomerAsync(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var OutcomesQuery = client.CreateDocumentQuery<Models.Outcomes>(collectionUri)
                .Where(so => so.CustomerId == customerId).AsDocumentQuery();

            var Outcomes = new List<Models.Outcomes>();

            while (OutcomesQuery.HasMoreResults)
            {
                var response = await OutcomesQuery.ExecuteNextAsync<Models.Outcomes>();
                Outcomes.AddRange(response);
            }

            return Outcomes.Any() ? Outcomes : null;
        }

        public async Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid OutcomesId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var OutcomesForCustomerQuery = client
                ?.CreateDocumentQuery<Models.Outcomes>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && 
                        x.ActionPlanId == actionplanId && 
                        x.OutcomesId == OutcomesId)
                .AsDocumentQuery();

            if (OutcomesForCustomerQuery == null)
                return null;

            var Outcomes = await OutcomesForCustomerQuery.ExecuteNextAsync<Models.Outcomes>();

            return Outcomes?.FirstOrDefault();
        }

        public async Task<ResourceResponse<Document>> CreateOutcomesAsync(Models.Outcomes Outcomes)
        {

            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, Outcomes);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateOutcomesAsync(Models.Outcomes Outcomes)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(Outcomes.OutcomesId.GetValueOrDefault());

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, Outcomes);

            return response;
        }
    }
}