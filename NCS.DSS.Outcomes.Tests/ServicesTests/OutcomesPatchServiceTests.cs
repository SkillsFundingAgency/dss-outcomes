using System;
using DFC.JSON.Standard;
using NCS.DSS.Outcomes.Models;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.ReferenceData;
using Newtonsoft.Json;
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
            var outcomePatch = new OutcomesPatch { SubcontractorId = "0000000111" };

            var patchedOutcomes = _outcomePatchService.Patch(_json, outcomePatch);

            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);

            // Assert
            Assert.AreEqual("0000000111", outcome.SubcontractorId);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckOutcomeTypeIsUpdated_WhenPatchIsCalled()
        {
            var outcomePatch = new OutcomesPatch { OutcomeType = OutcomeType.CareerProgression };

            var patchedOutcomes = _outcomePatchService.Patch(_json, outcomePatch);

            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);
            
            // Assert
            Assert.AreEqual(OutcomeType.CareerProgression, outcome.OutcomeType);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckOutcomeClaimedDateIsUpdated_WhenPatchIsCalled()
        {
            var outcomePatch = new OutcomesPatch { OutcomeClaimedDate = DateTime.MaxValue };

            var patchedOutcomes = _outcomePatchService.Patch(_json, outcomePatch);

            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, outcome.OutcomeClaimedDate);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckOutcomeEffectiveDateIsUpdated_WhenPatchIsCalled()
        {
            var outcomePatch = new Models.OutcomesPatch { OutcomeEffectiveDate = DateTime.MaxValue };

            var patchedOutcomes = _outcomePatchService.Patch(_json, outcomePatch);

            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, outcome.OutcomeEffectiveDate);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckClaimedPriorityGroupIsUpdated_WhenPatchIsCalled()
        {
            var outcomePatch = new OutcomesPatch { ClaimedPriorityGroup = ClaimedPriorityGroup.NotAPriorityCustomer };

            var patchedOutcomes = _outcomePatchService.Patch(_json, outcomePatch);

            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);

            // Assert
            Assert.AreEqual(ClaimedPriorityGroup.NotAPriorityCustomer, outcome.ClaimedPriorityGroup);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckTouchpointId_WhenPatchIsCalled()
        {
            var outcomePatch = new OutcomesPatch { TouchpointId = "0000000111" };

            var patchedOutcomes = _outcomePatchService.Patch(_json, outcomePatch);

            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);

            // Assert
            Assert.AreEqual("0000000111", outcome.TouchpointId);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var outcomePatch = new OutcomesPatch { LastModifiedDate = DateTime.MaxValue };

            var patchedOutcomes = _outcomePatchService.Patch(_json, outcomePatch);

            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);
            
            // Assert
            Assert.AreEqual(DateTime.MaxValue, outcome.LastModifiedDate);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckLastModifiedByUpdated_WhenPatchIsCalled()
        {
            var outcomePatch = new OutcomesPatch { LastModifiedTouchpointId = "0000000111" };

            var patchedOutcomes = _outcomePatchService.Patch(_json, outcomePatch);

            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);

            // Assert
            Assert.AreEqual("0000000111", outcome.LastModifiedTouchpointId);
        }
        
    }
}