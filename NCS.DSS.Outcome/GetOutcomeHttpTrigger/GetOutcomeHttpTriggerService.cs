using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Outcome.ReferenceData;

namespace NCS.DSS.Outcome.GetOutcomeHttpTrigger
{
    public class GetOutcomeHttpTriggerService
    {
        public async Task<List<Models.Outcome>> GetOutcomes()
        {
            var result = CreateTempOutcomes();
            return await Task.FromResult(result);
        }

        public List<Models.Outcome> CreateTempOutcomes()
        {
            var outcomeList = new List<Models.Outcome>
            {
                new Models.Outcome
                {
                    OutcomeId = Guid.Parse("187de7d1-292f-43e8-b936-f28f65857c24"),
                    ActionPlanId = Guid.NewGuid(),
                    OutcomeType = OutcomeType.CareersManagementOutcome,
                    OutcomeClaimedDate = DateTime.Today,
                    OutcomeEffectiveDate = DateTime.Today.AddYears(1),
                    TouchpointId = Guid.NewGuid(),
                    LastModifiedDate = DateTime.Today.AddYears(-1),
                    LastModifiedTouchpointId = Guid.NewGuid()
                },
                new Models.Outcome
                {
                    OutcomeId = Guid.Parse("cc8cf9f0-68e5-489b-91a2-7b9978fc0605"),
                    ActionPlanId = Guid.NewGuid(),
                    OutcomeType = OutcomeType.CustomerSatisfactionOutcome,
                    OutcomeClaimedDate = DateTime.Today,
                    OutcomeEffectiveDate = DateTime.Today.AddYears(2),
                    TouchpointId = Guid.NewGuid(),
                    LastModifiedDate = DateTime.Today.AddYears(-2),
                    LastModifiedTouchpointId = Guid.NewGuid()
                },
                new Models.Outcome
                {
                    OutcomeId = Guid.Parse("2ee14c94-5a4a-425a-85d5-a579591b29d8"),
                    ActionPlanId = Guid.NewGuid(),
                    OutcomeType = OutcomeType.JobOutcome,
                    OutcomeClaimedDate = DateTime.Today,
                    OutcomeEffectiveDate = DateTime.Today.AddYears(3),
                    TouchpointId = Guid.NewGuid(),
                    LastModifiedDate = DateTime.Today.AddYears(-3),
                    LastModifiedTouchpointId = Guid.NewGuid()
                }
            };

            return outcomeList;
        }

    }
}
