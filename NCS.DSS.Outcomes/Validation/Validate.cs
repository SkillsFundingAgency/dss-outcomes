using NCS.DSS.Outcomes.Models;
using NCS.DSS.Outcomes.ReferenceData;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Outcomes.Validation
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(IOutcomes resource, DateTime? dateAndTimeSessionCreated)
        {
            var context = new ValidationContext(resource, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(resource, context, results, true);
            ValidateOutcomesRules(resource, results, dateAndTimeSessionCreated);

            return results;
        }

        private void ValidateOutcomesRules(IOutcomes outcomesResource, List<ValidationResult> results, DateTime? dateAndTimeSessionCreated)
        {
            if (outcomesResource == null)
                return;


            if (outcomesResource.OutcomeClaimedDate.HasValue)
            {
                if (outcomesResource.OutcomeClaimedDate.Value > DateTime.UtcNow)
                    results.Add(new ValidationResult("Outcome Claimed Date must be less the current date/time", new[] { "OutcomeClaimedDate" }));

                if (!(outcomesResource.OutcomeClaimedDate >= outcomesResource.OutcomeEffectiveDate.GetValueOrDefault()))
                    results.Add(new ValidationResult("Outcome Claimed Date must be greater than Outcome Effective Date", new[] { "OutcomeClaimedDate" }));

                if (!outcomesResource.OutcomeEffectiveDate.HasValue)
                    results.Add(new ValidationResult("Please supply a Outcome Effective Date", new[] { "OutcomeEffectiveDate" }));

            }

            if (outcomesResource.OutcomeEffectiveDate.HasValue)
            {
                if (outcomesResource.OutcomeEffectiveDate.Value > DateTime.UtcNow)
                    results.Add(new ValidationResult("Outcome Effective Date Completed must be less the current date/time", new[] { "OutcomeEffectiveDate" }));

                if (outcomesResource.OutcomeType.HasValue && dateAndTimeSessionCreated.HasValue)
                {
                    switch (outcomesResource.OutcomeType)
                    {
                        case OutcomeType.CustomerSatisfaction:
                        case OutcomeType.CareersManagement:
                        case OutcomeType.AccreditedLearning:
                            if (!(outcomesResource.OutcomeEffectiveDate.Value >= dateAndTimeSessionCreated &&
                                  outcomesResource.OutcomeEffectiveDate.Value <=
                                  dateAndTimeSessionCreated.Value.AddMonths(12)))
                                results.Add(new ValidationResult(
                                    "Outcome Effective Date Completed must be within 12 months of Date Time Session Created",
                                    new[] { "OutcomeEffectiveDate" }));
                            break;
                        case OutcomeType.SustainableEmployment:
                        case OutcomeType.CareerProgression:
                            if (!(outcomesResource.OutcomeEffectiveDate.Value >= dateAndTimeSessionCreated &&
                                  outcomesResource.OutcomeEffectiveDate.Value <=
                                  dateAndTimeSessionCreated.Value.AddMonths(13)))
                                results.Add(new ValidationResult(
                                    "Outcome Effective Date Completed must be within 13 months of Date Time Session Created",
                                    new[] { "OutcomeEffectiveDate" }));
                            break;
                    }
                }
            }

            if (outcomesResource.LastModifiedDate.HasValue && outcomesResource.LastModifiedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Last Modified Date must be less the current date/time", new[] { "LastModifiedDate" }));

            if (outcomesResource.OutcomeType.HasValue && !Enum.IsDefined(typeof(OutcomeType), outcomesResource.OutcomeType.Value))
                results.Add(new ValidationResult("Please supply a valid OutcomeType", new[] { "OutcomeType" }));
        }
    }
}
