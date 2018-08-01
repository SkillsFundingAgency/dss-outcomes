using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Outcomes.Models;

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

            if (outcomesResource.OutcomeEffectiveDate.HasValue && outcomesResource.OutcomeEffectiveDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Outcome Effective Date Completed must be less the current date/time", new[] { "OutcomeEffectiveDate" }));

            if (outcomesResource.LastModifiedDate.HasValue && outcomesResource.LastModifiedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Last Modified Date must be less the current date/time", new[] { "LastModifiedDate" }));
   
        }

    }
}
