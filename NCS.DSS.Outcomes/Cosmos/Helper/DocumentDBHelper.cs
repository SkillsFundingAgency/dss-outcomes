
using System;
using System.Configuration;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Outcomes.Cosmos.Helper
{
    public static class DocumentDBHelper
    {
        private static Uri _documentCollectionUri;
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["DatabaseId"];
        private static readonly string CollectionId = ConfigurationManager.AppSettings["CollectionId"];

        private static Uri _customerDocumentCollectionUri;
        private static readonly string CustomerDatabaseId = ConfigurationManager.AppSettings["CustomerDatabaseId"];
        private static readonly string CustomerCollectionId = ConfigurationManager.AppSettings["CustomerCollectionId"];

        private static Uri _interactionDocumentCollectionUri;
        private static readonly string InteractionDatabaseId = ConfigurationManager.AppSettings["InteractionDatabaseId"];
        private static readonly string InteractionCollectionId = ConfigurationManager.AppSettings["InteractionCollectionId"];

        private static Uri _actionplanDocumentCollectionUri;
        private static readonly string ActionPlanDatabaseId = ConfigurationManager.AppSettings["ActionPlanDatabaseId"];
        private static readonly string ActionPlanCollectionId = ConfigurationManager.AppSettings["ActionPlanCollectionId"];

        public static Uri CreateDocumentCollectionUri()
        {
            if (_documentCollectionUri != null)
                return _documentCollectionUri;

            _documentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                DatabaseId,
                CollectionId);

            return _documentCollectionUri;
        }
        
        public static Uri CreateDocumentUri(Guid outcomeId)
        {
           return UriFactory.CreateDocumentUri(DatabaseId, CollectionId, outcomeId.ToString());
        }

        #region CustomerDB

        public static Uri CreateCustomerDocumentCollectionUri()
        {
            if (_customerDocumentCollectionUri != null)
                return _customerDocumentCollectionUri;

            _customerDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                CustomerDatabaseId, CustomerCollectionId);

            return _customerDocumentCollectionUri;
        }

        public static Uri CreateCustomerDocumentUri(Guid customerId)
        {
            return UriFactory.CreateDocumentUri(CustomerDatabaseId, CustomerCollectionId, customerId.ToString());
        }

        #endregion

        #region InteractionDB

        public static Uri CreateInteractionDocumentCollectionUri()
        {
            if (_interactionDocumentCollectionUri != null)
                return _interactionDocumentCollectionUri;

            _interactionDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                InteractionDatabaseId, InteractionCollectionId);

            return _interactionDocumentCollectionUri;
        }

        public static Uri CreateInteractionDocumentUri(Guid interactionId)
        {
            return UriFactory.CreateDocumentUri(InteractionDatabaseId, InteractionCollectionId, interactionId.ToString()); ;
        }

        #endregion

        #region ActionPlanDB

        public static Uri CreateActionPlanDocumentCollectionUri()
        {
            if (_actionplanDocumentCollectionUri != null)
                return _actionplanDocumentCollectionUri;

            _actionplanDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(ActionPlanDatabaseId, ActionPlanCollectionId);

            return _actionplanDocumentCollectionUri;
        }

        public static Uri CreateActionPlanDocumentUri(Guid actionPlanId)
        {
            return UriFactory.CreateDocumentUri(ActionPlanDatabaseId, ActionPlanCollectionId, actionPlanId.ToString());
        }
        #endregion   


    }
}
