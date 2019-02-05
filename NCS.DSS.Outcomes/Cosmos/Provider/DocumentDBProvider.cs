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
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Outcomes.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);
                if (response.Resource != null)
                    return true;
            }
            catch (DocumentClientException)
            {
                return false;
            }

            return false;
        }

        public bool DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateInteractionDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var query = client.CreateDocumentQuery<long>(collectionUri, new SqlQuerySpec()
                {
                    QueryText = "SELECT VALUE COUNT(1) FROM interactions i " +
                                "WHERE i.id = @interactionId " +
                                "AND i.CustomerId = @customerId",

                    Parameters = new SqlParameterCollection()
                    {
                        new SqlParameter("@interactionId", interactionId),
                        new SqlParameter("@customerId", customerId)
                    }
                }).AsEnumerable().FirstOrDefault();

                return Convert.ToBoolean(Convert.ToInt16(query));
            }
            catch (DocumentQueryException)
            {
                return false;
            }

        }

        public bool DoesActionPlanResourceExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateActionPlanDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var query = client.CreateDocumentQuery<long>(collectionUri, new SqlQuerySpec()
                {
                    QueryText = "SELECT VALUE COUNT(1) FROM actionplans a " +
                                "WHERE a.id = @actionPlanId " +
                                "AND a.InteractionId = @interactionId " +
                                "AND a.CustomerId = @customerId",

                    Parameters = new SqlParameterCollection()
                    {
                        new SqlParameter("@actionPlanId", actionPlanId),
                        new SqlParameter("@interactionId", interactionId),
                        new SqlParameter("@customerId", customerId)
                    }
                }).AsEnumerable().FirstOrDefault();

                return Convert.ToBoolean(Convert.ToInt16(query));
            }
            catch (DocumentQueryException)
            {
                return false;
            }

        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);

                var dateOfTermination = response.Resource?.GetPropertyValue<DateTime?>("DateOfTermination");

                return dateOfTermination.HasValue;
            }
            catch (DocumentClientException)
            {
                return false;
            }
        }

        public async Task<List<Models.Outcomes>> GetOutcomesForCustomerAsync(Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

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

        public async Task<Models.Outcomes> GetOutcomesForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid outcomeId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var outcomesForCustomerQuery = client
                ?.CreateDocumentQuery<Models.Outcomes>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && 
                        x.ActionPlanId == actionplanId && 
                        x.OutcomeId == outcomeId)
                .AsDocumentQuery();

            if (outcomesForCustomerQuery == null)
                return null;

            var outcomes = await outcomesForCustomerQuery.ExecuteNextAsync<Models.Outcomes>();

            return outcomes?.FirstOrDefault();
        }

        public async Task<string> GetOutcomesForCustomerAsyncToUpdateAsync(Guid customerId, Guid interactionsId, Guid actionPlanId, Guid outcomeId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var outcomesForCustomerQuery = client
                ?.CreateDocumentQuery<Models.Outcomes>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId &&
                            x.ActionPlanId == actionPlanId &&
                            x.OutcomeId == outcomeId)
                .AsDocumentQuery();

            if (outcomesForCustomerQuery == null)
                return null;

            var outcomes = await outcomesForCustomerQuery.ExecuteNextAsync();

            return outcomes?.FirstOrDefault()?.ToString();
        }

        public async Task<ResourceResponse<Document>> CreateOutcomesAsync(Models.Outcomes outcomes)
        {

            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, outcomes);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateOutcomesAsync(string outcomesJson, Guid outcomeId)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(outcomeId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var outcomeJsonObject = JObject.Parse(outcomesJson);

            var response = await client.ReplaceDocumentAsync(documentUri, outcomeJsonObject);

            return response;
        }

        public async Task<bool> DeleteAsync(Guid outcomeId)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(outcomeId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            var response = await client.DeleteDocumentAsync(documentUri);

            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}