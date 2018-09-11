using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateCustomerDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var customerByIdQuery = client
                ?.CreateDocumentQuery<Document>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.Id == customerId.ToString())
                .AsDocumentQuery();

            if (customerByIdQuery == null)
                return false;

            var customerQuery = await customerByIdQuery.ExecuteNextAsync<Document>();

            var customer = customerQuery?.FirstOrDefault();

            if (customer == null)
                return false;

            var dateOfTermination = customer.GetPropertyValue<DateTime?>("DateOfTermination");

            return dateOfTermination.HasValue;
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

            var outcomesQuery = client.CreateDocumentQuery<Models.Outcomes>(collectionUri)
                .Where(so => so.CustomerId == customerId).AsDocumentQuery();

            var outcomes = new List<Models.Outcomes>();

            while (outcomesQuery.HasMoreResults)
            {
                var response = await outcomesQuery.ExecuteNextAsync<Models.Outcomes>();
                outcomes.AddRange(response);
            }

            return outcomes.Any() ? outcomes : null;
        }

        public async Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid outcomesId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var outcomesForCustomerQuery = client
                ?.CreateDocumentQuery<Models.Outcomes>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && 
                        x.ActionPlanId == actionplanId && 
                        x.OutcomeId == outcomesId)
                .AsDocumentQuery();

            if (outcomesForCustomerQuery == null)
                return null;

            var outcomes = await outcomesForCustomerQuery.ExecuteNextAsync<Models.Outcomes>();

            return outcomes?.FirstOrDefault();
        }

        public async Task<ResourceResponse<Document>> CreateOutcomesAsync(Models.Outcomes outcomes)
        {

            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, outcomes);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateOutcomesAsync(Models.Outcomes outcomes)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(outcomes.OutcomeId.GetValueOrDefault());

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, outcomes);

            return response;
        }

        public async Task<bool> DeleteAsync(Guid outcomesId)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(outcomesId);

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var response = await client.DeleteDocumentAsync(documentUri);

            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}