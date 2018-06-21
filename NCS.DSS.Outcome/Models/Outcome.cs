using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Outcome.Annotations;
using NCS.DSS.Outcome.ReferenceData;

namespace NCS.DSS.Outcome.Models
{
    public class Outcome
    {
        [Display(Description = "Unique identifier for a diversity record.")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        public Guid OutcomeId { get; set; }

        [Required]
        [Display(Description = "Unique identifier of a action plan.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid ActionPlanId { get; set; }

        [Required]
        [Display(Description = "Outcome type reference data")]
        [Example(Description = "1")]
        public OutcomeType OutcomeType { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date that an outcome was claimed by the prime contractor.")]
        [Example(Description = "2018-06-21T93:45:00")]
        public DateTime OutcomeClaimedDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the primes were notified that the outcome had been achieved by the customer.")]
        [Example(Description = "2018-06-22T10:32:00")]
        public DateTime OutcomeEffectiveDate { get; set; }

        [Required]
        [Display(Description = "Identifier of the touchpoint claiming the outcome.")]
        [Example(Description = "0d47878c-de6c-4d61-a786-135db1f65a86")]
        public Guid TouchpointId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-23T13:15:00")]
        public DateTime LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record.")]
        [Example(Description = "905d618d-e934-4aea-a1e4-27ced31b27ca")]
        public Guid LastModifiedTouchpointId { get; set; }
    }
}