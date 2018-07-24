﻿using System;

namespace NCS.DSS.Outcomes.Cosmos.Helper
{
    public interface IDocumentDBHelper
    {
        Uri CreateDocumentCollectionUri();
        Uri CreateDocumentUri(Guid OutcomesId);
        Uri CreateCustomerDocumentCollectionUri();
        Uri CreateInteractionDocumentCollectionUri();
    }
}