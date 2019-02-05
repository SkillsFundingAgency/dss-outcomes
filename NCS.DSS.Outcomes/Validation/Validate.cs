using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Outcomes.Models;
using NCS.DSS.Outcomes.ReferenceData;

namespace NCS.DSS.Outcomes.Validation
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(IOutcomes resource)
        {
            var context = new ValidationContext(resource, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(resource, context, results, true);
            ValidateOutcomesRules(resource, results);

            return results;
        }

        private void ValidateOutcomesRules(IOutcomes outcomesResource, List<ValidationResult> results)
        {
            if (outcomesResource == null)
                return;

            if (outcomesResource.OutcomeClaimedDate.HasValue && outcomesResource.OutcomeClaimedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Outcome Claimed Date must be less the current date/time", new[] { "OutcomeClaimedDate" }));

            if(outcomesResource.OutcomeClaimedDate.HasValue && !outcomesResource.ClaimedPriorityGroupId.HasValue)
                results.Add(new ValidationResult("Please supply a Claimed Priority Group Id", new[] { "ClaimedPriorityGroupId" }));

            if (outcomesResource.OutcomeEffectiveDate.HasValue && outcomesResource.OutcomeEffectiveDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Outcome Effective Date Completed must be less the current date/time", new[] { "OutcomeEffectiveDate" }));

            if (outcomesResource.LastModifiedDate.HasValue && outcomesResource.LastModifiedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Last Modified Date must be less the current date/time", new[] { "LastModifiedDate" }));
            
            if (outcomesResource.OutcomeType.HasValue && !Enum.IsDefined(typeof(OutcomeType), outcomesResource.OutcomeType.Value))
                results.Add(new ValidationResult("Please supply a valid OutcomeType", new[] { "OutcomeType" }));

        }

    }
}
