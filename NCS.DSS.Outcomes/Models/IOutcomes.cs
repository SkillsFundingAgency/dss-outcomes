using System;
using System.Collections.Generic;
using NCS.DSS.Outcomes.ReferenceData;

namespace NCS.DSS.Outcomes.Models
{
    public interface IOutcomes
    {
        Guid? SessionId { get; set; }
        string SubcontractorId { get; set; }
        OutcomeType? OutcomeType { get; set; }
        DateTime? OutcomeClaimedDate { get; set; }
        DateTime? OutcomeEffectiveDate { get; set; }
        bool? IsPriorityCustomer { get; set; }
        string TouchpointId { get; set; }
        DateTime? LastModifiedDate { get; set; }
        string LastModifiedTouchpointId { get; set; }

        void SetDefaultValues();
    }
}