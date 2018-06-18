using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Outcome.ReferenceData;

namespace NCS.DSS.Outcome.Models
{
    public class Outcome
    {
        [Display(Description = "Unique identifier for a diversity record.")]
        public Guid OutcomeId { get; set; }

        [Required]
        [Display(Description = "Unique identifier of a action plan.")]
        public Guid ActionPlanId { get; set; }

        [Required]
        [Display(Description = "Outcome type reference data")]
        public OutcomeType OutcomeType { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date that an outcome was claimed by the prime contractor.")]
        public DateTime OutcomeClaimedDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the primes were notified that the outcome had been achieved by the customer.")]
        public DateTime OutcomeEffectiveDate { get; set; }

        [Required]
        [Display(Description = "Identifier of the touchpoint claiming the outcome.")]
        public Guid TouchpointId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        public DateTime LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record.")]
        public Guid LastModifiedTouchpointId { get; set; }
    }
}