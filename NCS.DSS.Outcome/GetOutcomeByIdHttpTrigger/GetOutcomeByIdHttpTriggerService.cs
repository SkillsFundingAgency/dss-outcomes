using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCS.DSS.Outcome.GetOutcomeByIdHttpTrigger
{
    public class GetOutcomeByIdHttpTriggerService
    {
        public async Task<Models.Outcome> GetOutcome(Guid outcomeId)
        {
            var outcomes = CreateTempOutcomes();
            var result = outcomes.FirstOrDefault(a => a.OutcomeId == outcomeId);
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
                    OutcomeTypeId = 1,
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
                    OutcomeTypeId = 2,
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
                    OutcomeTypeId = 3,
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