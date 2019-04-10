using NCS.DSS.Outcomes.Models;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service
{
    public interface IOutcomePatchService
    {
        string Patch(string outcomeJson, OutcomesPatch outcomePatch);

        string SetOutcomeClaimedDateOrOutcomeEffectiveDateToNull(string outcomeJson, bool setOutcomeClaimedDateToNull,
            bool setOutcomeEffectiveDateToNull);
    }
}
