using NCS.DSS.Outcomes.Models;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service
{
    public interface IOutcomePatchService
    {
        Models.Outcomes Patch(string outcomeJson, OutcomesPatch outcomePatch);
    }
}
