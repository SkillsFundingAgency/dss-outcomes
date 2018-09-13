using System;

namespace NCS.DSS.Outcomes.Cosmos.Helper
{
    public interface IDocumentDBHelper
    {
        Uri CreateDocumentCollectionUri();
        Uri CreateDocumentUri(Guid OutcomeId);
        Uri CreateCustomerDocumentCollectionUri();
        Uri CreateInteractionDocumentCollectionUri();
    }
}