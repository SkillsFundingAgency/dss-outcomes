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
            var diversityPatch = new OutcomesPatch { SubcontractorId = "0000000111" };

            var outcomes = _outcomePatchService.Patch(_json, diversityPatch);
            
            var subcontractorId = outcomes.SubcontractorId;

            // Assert
            Assert.AreEqual("0000000111", subcontractorId);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckOutcomeTypeIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new OutcomesPatch { OutcomeType = OutcomeType.CareerProgression };

            var outcome = _outcomePatchService.Patch(_json, diversityPatch);

            var outcomeType = outcome.OutcomeType;
            
            // Assert
            Assert.AreEqual(OutcomeType.CareerProgression, outcomeType);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckOutcomeClaimedDateIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new OutcomesPatch { OutcomeClaimedDate = DateTime.MaxValue };

            var outcome = _outcomePatchService.Patch(_json, diversityPatch);

            var outcomeClaimedDate = outcome.OutcomeClaimedDate;

            // Assert
            Assert.AreEqual(DateTime.MaxValue, outcomeClaimedDate);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckOutcomeEffectiveDateIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.OutcomesPatch { OutcomeEffectiveDate = DateTime.MaxValue };

            var outcome = _outcomePatchService.Patch(_json, diversityPatch);

            var outcomeEffectiveDate = outcome.OutcomeEffectiveDate;

            // Assert
            Assert.AreEqual(DateTime.MaxValue, outcomeEffectiveDate);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckClaimedPriorityGroupIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new OutcomesPatch { ClaimedPriorityGroup = ClaimedPriorityGroup.NotAPriorityCustomer };

            var outcome = _outcomePatchService.Patch(_json, diversityPatch);

            var claimedPriorityGroup = outcome.ClaimedPriorityGroup;

            // Assert
            Assert.AreEqual(ClaimedPriorityGroup.NotAPriorityCustomer, claimedPriorityGroup);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckTouchpointId_WhenPatchIsCalled()
        {
            var diversityPatch = new OutcomesPatch { TouchpointId = "0000000111" };

            var outcome = _outcomePatchService.Patch(_json, diversityPatch);

            var touchpointId = outcome.TouchpointId;
            
            // Assert
            Assert.AreEqual("0000000111", touchpointId);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new OutcomesPatch { LastModifiedDate = DateTime.MaxValue };

            var outcome = _outcomePatchService.Patch(_json, diversityPatch);

            var lastModifiedDate = outcome.LastModifiedDate;
            
            // Assert
            Assert.AreEqual(DateTime.MaxValue, lastModifiedDate);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckLastModifiedByUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new OutcomesPatch { LastModifiedTouchpointId = "0000000111" };

            var outcome = _outcomePatchService.Patch(_json, diversityPatch);

            var lastModifiedTouchpointId = outcome.LastModifiedTouchpointId;

            // Assert
            Assert.AreEqual("0000000111", lastModifiedTouchpointId);
        }
        
    }
}