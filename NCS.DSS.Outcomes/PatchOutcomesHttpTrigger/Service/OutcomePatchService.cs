using DFC.JSON.Standard;
using NCS.DSS.Outcomes.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service
{
    public class OutcomePatchService : IOutcomePatchService
    {
        private IJsonHelper _jsonHelper;

        public OutcomePatchService(IJsonHelper jsonHelper)
        {
            _jsonHelper = jsonHelper;
        }

        public Models.Outcomes Patch(string outcomeJson, OutcomesPatch outcomePatch)
        {
            if (string.IsNullOrEmpty(outcomeJson))
                return null;

            var obj = JObject.Parse(outcomeJson);

            if (!string.IsNullOrEmpty(outcomePatch.SubcontractorId))
            {
                if (obj["SubcontractorId"] == null)
                    _jsonHelper.CreatePropertyOnJObject(obj, "SubcontractorId", outcomePatch.SubcontractorId);
                else
                    _jsonHelper.UpdatePropertyValue(obj["SubcontractorId"], outcomePatch.SubcontractorId);
            }

            if (outcomePatch.OutcomeType.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["OutcomeType"], outcomePatch.OutcomeType);

            if (outcomePatch.OutcomeClaimedDate.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["OutcomeClaimedDate"], outcomePatch.OutcomeClaimedDate.Value);

            if (outcomePatch.OutcomeEffectiveDate.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["OutcomeEffectiveDate"], outcomePatch.OutcomeEffectiveDate);

            if (outcomePatch.ClaimedPriorityGroup.HasValue)
            {
                if (obj["ClaimedPriorityGroup"] == null)
                    _jsonHelper.CreatePropertyOnJObject(obj, "ClaimedPriorityGroup", outcomePatch.ClaimedPriorityGroup);
                else
                    _jsonHelper.UpdatePropertyValue(obj["ClaimedPriorityGroup"], outcomePatch.ClaimedPriorityGroup);
            }

            if (!string.IsNullOrEmpty(outcomePatch.TouchpointId))
                _jsonHelper.UpdatePropertyValue(obj["TouchpointId"], outcomePatch.TouchpointId);

            if (outcomePatch.LastModifiedDate.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], outcomePatch.LastModifiedDate);

            if (!string.IsNullOrEmpty(outcomePatch.LastModifiedTouchpointId))
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], outcomePatch.LastModifiedTouchpointId);

            return obj.ToObject<Models.Outcomes>();

        }
    }
}
