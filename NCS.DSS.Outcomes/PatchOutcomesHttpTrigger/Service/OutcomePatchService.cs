using NCS.DSS.Outcomes.Helpers;
using NCS.DSS.Outcomes.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service
{
    public class OutcomePatchService : IOutcomePatchService
    {
        public string Patch(string outcomeJson, OutcomesPatch outcomePatch)
        {
            if (string.IsNullOrEmpty(outcomeJson))
                return null;

            var obj = JObject.Parse(outcomeJson);

            if (outcomePatch.OutcomeType.HasValue)
                JsonHelper.UpdatePropertyValue(obj["OutcomeType"], outcomePatch.OutcomeType);

            if (outcomePatch.OutcomeClaimedDate.HasValue)
                JsonHelper.UpdatePropertyValue(obj["OutcomeClaimedDate"], outcomePatch.OutcomeClaimedDate.Value);

            if (outcomePatch.OutcomeEffectiveDate.HasValue)
                JsonHelper.UpdatePropertyValue(obj["OutcomeEffectiveDate"], outcomePatch.OutcomeEffectiveDate);

           if (!string.IsNullOrEmpty(outcomePatch.TouchpointId))
                JsonHelper.UpdatePropertyValue(obj["TouchpointId"], outcomePatch.TouchpointId);

            if (outcomePatch.LastModifiedDate.HasValue)
                JsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], outcomePatch.LastModifiedDate);

            if (!string.IsNullOrEmpty(outcomePatch.LastModifiedTouchpointId))
                JsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], outcomePatch.LastModifiedTouchpointId);

            return obj.ToString();

        }

    }
}