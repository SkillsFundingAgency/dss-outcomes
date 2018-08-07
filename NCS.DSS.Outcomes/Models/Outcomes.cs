using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Outcomes.Annotations;
using NCS.DSS.Outcomes.ReferenceData;

namespace NCS.DSS.Outcomes.Models
{
    public class Outcomes : IOutcomes
    {
        [Display(Description = "Unique identifier of the Outcomes record.")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public Guid? OutcomesId { get; set; }

        [Required]
        [Display(Description = "Unique identifier of a customer.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid? CustomerId { get; set; }

        [Required]
        [Display(Description = "Unique identifier to the related action plan resource.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid? ActionPlanId { get; set; }

        [Display(Description = "Must be a valid Outcome Type reference data value.")]
        [Example(Description = "1")]
        public OutcomeType? OutcomeType { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Description = "Date that an outcome was claimed by the prime contractor.  Only one Outcome of each type is allowed within a 12 month period")]
        [Example(Description = "2018-06-20T21:45:00")]
        public DateTime? OutcomeClaimedDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Description = "Date the primes were notified that the outcome had been achieved by the customer")]
        [Example(Description = "2018-06-20T21:45:00")]
        public DateTime? OutcomeEffectiveDate { get; set; }

        [Display(Description = "Identifier of the touchpoint claiming the outcome.")]
        [Example(Description = "d1307d77-af23-4cb4-b600-a60e04f8c3df")]
        public Guid? TouchpointId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-20T13:45:00")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedTouchpointId { get; set; }

        public void SetDefaultValues()
        {
            OutcomesId = Guid.NewGuid();

            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;
        }

        public void Patch(OutcomesPatch outcomesPatch)
        {
            if (outcomesPatch == null)
                return;

            if(outcomesPatch.OutcomeType.HasValue)
                OutcomeType = outcomesPatch.OutcomeType;
            
            if (outcomesPatch.OutcomeClaimedDate.HasValue)
                OutcomeClaimedDate = outcomesPatch.OutcomeClaimedDate.Value;

            if (outcomesPatch.OutcomeEffectiveDate.HasValue)
                OutcomeEffectiveDate = outcomesPatch.OutcomeEffectiveDate;

            if (outcomesPatch.TouchpointId.HasValue)
                TouchpointId = outcomesPatch.TouchpointId.Value;

            if (outcomesPatch.LastModifiedDate.HasValue)
                LastModifiedDate = outcomesPatch.LastModifiedDate;

            if (!string.IsNullOrEmpty(outcomesPatch.LastModifiedTouchpointId))
                LastModifiedTouchpointId = outcomesPatch.LastModifiedTouchpointId;

        }
    }
}