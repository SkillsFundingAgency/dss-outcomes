using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Outcome.Models
{
    public class Outcome
    {
        public Guid OutcomeId { get; set; }

        [Required]
        public Guid ActionPlanId { get; set; }

        [Required]
        public int OutcomeTypeId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime OutcomeClaimedDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime OutcomeEffectiveDate { get; set; }

        [Required]
        public Guid TouchpointId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastModifiedDate { get; set; }

        public Guid LastModifiedTouchpointId { get; set; }
    }
}