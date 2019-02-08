using System;
using DFC.JSON.Standard;
using NCS.DSS.Outcomes.Models;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.ReferenceData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Outcomes.Tests.ServicesTests
{

    [TestFixture]
    public class OutcomesPatchServiceTests
    {
        private IJsonHelper _jsonHelper;
        private OutcomePatchService _outcomePatchService;
        private OutcomesPatch _outcomePatch;
        private string _json;

        [SetUp]
        public void Setup()
        {
            _jsonHelper = Substitute.For<JsonHelper>();
            _outcomePatchService = Substitute.For<OutcomePatchService>(_jsonHelper);
            _outcomePatch = Substitute.For<OutcomesPatch>();

            _json = JsonConvert.SerializeObject(_outcomePatch);
        }

        [Test]
        public void OutcomesPatchServiceTests_ReturnsNull_WhenOutcomePatchIsNull()
        {
            var result = _outcomePatchService.Patch(string.Empty, Arg.Any<OutcomesPatch>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckSubcontractorIdIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new OutcomesPatch { SubcontractorId = "0000000111" };

            var updated = _outcomePatchService.Patch(_json, diversityPatch);

            var jsonObject = (JObject)JsonConvert.DeserializeObject(updated);

            var subcontractorId = jsonObject["SubcontractorId"].ToString();

            // Assert
            Assert.AreEqual("0000000111", subcontractorId);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckOutcomeTypeIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new OutcomesPatch { OutcomeType = OutcomeType.CareerProgression };

            var updated = _outcomePatchService.Patch(_json, diversityPatch);

            var jsonObject = (JObject)JsonConvert.DeserializeObject(updated);

            var outcomeType = (OutcomeType)int.Parse(jsonObject["OutcomeType"].ToString());
            
            // Assert
            Assert.AreEqual(OutcomeType.CareerProgression, outcomeType);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckOutcomeClaimedDateIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new OutcomesPatch { OutcomeClaimedDate = DateTime.MaxValue };

            var updated = _outcomePatchService.Patch(_json, diversityPatch);

            var jsonObject = (JObject)JsonConvert.DeserializeObject(updated);

            var outcomeClaimedDate = (DateTime)jsonObject["OutcomeClaimedDate"];

            // Assert
            Assert.AreEqual(DateTime.MaxValue, outcomeClaimedDate);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckOutcomeEffectiveDateIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.OutcomesPatch { OutcomeEffectiveDate = DateTime.MaxValue };

            var updated = _outcomePatchService.Patch(_json, diversityPatch);

            var jsonObject = (JObject)JsonConvert.DeserializeObject(updated);

            var outcomeEffectiveDate = (DateTime)jsonObject["OutcomeEffectiveDate"];

            // Assert
            Assert.AreEqual(DateTime.MaxValue, outcomeEffectiveDate);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckClaimedPriorityGroupIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new OutcomesPatch { ClaimedPriorityGroup = ClaimedPriorityGroup.NotAPriorityCustomer };

            var updated = _outcomePatchService.Patch(_json, diversityPatch);

            var jsonObject = (JObject)JsonConvert.DeserializeObject(updated);

            var claimedPriorityGroup = (ClaimedPriorityGroup)int.Parse(jsonObject["ClaimedPriorityGroup"].ToString());

            // Assert
            Assert.AreEqual(ClaimedPriorityGroup.NotAPriorityCustomer, claimedPriorityGroup);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckTouchpointId_WhenPatchIsCalled()
        {
            var diversityPatch = new OutcomesPatch { TouchpointId = "0000000111" };

            var updated = _outcomePatchService.Patch(_json, diversityPatch);

            var jsonObject = (JObject)JsonConvert.DeserializeObject(updated);

            var touchpointId = jsonObject["TouchpointId"].ToString();
            
            // Assert
            Assert.AreEqual("0000000111", touchpointId);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new OutcomesPatch { LastModifiedDate = DateTime.MaxValue };

            var updated = _outcomePatchService.Patch(_json, diversityPatch);

            var jsonObject = (JObject)JsonConvert.DeserializeObject(updated);

            var lastModifiedDate = (DateTime)jsonObject["LastModifiedDate"];
            
            // Assert
            Assert.AreEqual(DateTime.MaxValue, lastModifiedDate);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckLastModifiedByUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new OutcomesPatch { LastModifiedTouchpointId = "0000000111" };

            var updated = _outcomePatchService.Patch(_json, diversityPatch);

            var jsonObject = (JObject)JsonConvert.DeserializeObject(updated);

            var lastModifiedTouchpointId = jsonObject["LastModifiedTouchpointId"].ToString();

            // Assert
            Assert.AreEqual("0000000111", lastModifiedTouchpointId);
        }
        
    }
}