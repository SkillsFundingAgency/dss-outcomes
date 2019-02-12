using System;
using NCS.DSS.Outcomes.ReferenceData;

namespace NCS.DSS.Outcomes.Models
{
    public interface IOutcomes
    {
        Guid? OutcomeId { get; set; }
        Guid? CustomerId { get; set; }
        Guid? ActionPlanId { get; set; }
        OutcomeType? OutcomeType { get; set; }
        DateTime? OutcomeClaimedDate { get; set; }
        DateTime? OutcomeEffectiveDate { get; set; }
        ClaimedPriorityGroup? ClaimedPriorityGroup { get; set; }
        string TouchpointId { get; set; }
        DateTime? LastModifiedDate { get; set; }
        string LastModifiedTouchpointId { get; set; }

        void SetDefaultValues();
    }
}