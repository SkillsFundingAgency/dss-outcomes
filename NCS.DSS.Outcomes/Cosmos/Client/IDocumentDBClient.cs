using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Outcomes.Cosmos.Client
{
    public interface IDocumentDBClient
    {
        DocumentClient CreateDocumentClient();
    }
}