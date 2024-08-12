using DFC.JSON.Standard;
using Moq;
using NCS.DSS.Outcomes.Models;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.ReferenceData;
using Newtonsoft.Json;
using NUnit.Framework;
using System;

namespace NCS.DSS.Outcomes.Tests.ServicesTests
{
    [TestFixture]
    public class OutcomesPatchServiceTests
    {
        private Mock<IJsonHelper> jsonHelper;
        private IOutcomePatchService _outcomePatchService;

        [SetUp]
        public void Setup()
        {
            jsonHelper = new Mock<IJsonHelper>();
            _outcomePatchService = new OutcomePatchService(jsonHelper.Object);
        }

        [Test]
        public void OutcomesPatchServiceTests_ReturnsNull_WhenOutcomePatchIsNull()
        {
            // Arrange

            // Act
            var result = _outcomePatchService.Patch(string.Empty, It.IsAny<OutcomesPatch>());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckSubcontractorIdIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var outcomePatch = new OutcomesPatch { SubcontractorId = "0000000111" };
            var json = JsonConvert.SerializeObject(outcomePatch);

            // Act
            var patchedOutcomes = _outcomePatchService.Patch(json, outcomePatch);
            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);

            // Assert
            Assert.That(outcome.SubcontractorId, Is.EqualTo("0000000111"));
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckOutcomeTypeIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var outcomePatch = new OutcomesPatch { OutcomeType = OutcomeType.CareerProgression };
            var json = JsonConvert.SerializeObject(outcomePatch);

            // Act
            var patchedOutcomes = _outcomePatchService.Patch(json, outcomePatch);
            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);
            
            // Assert
            Assert.That(outcome.OutcomeType, Is.EqualTo(OutcomeType.CareerProgression));
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckOutcomeClaimedDateIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var outcomePatch = new OutcomesPatch { OutcomeClaimedDate = DateTime.MaxValue };
            var json = JsonConvert.SerializeObject(outcomePatch);

            // Act
            var patchedOutcomes = _outcomePatchService.Patch(json, outcomePatch);
            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);

            // Assert
            Assert.That(outcome.OutcomeClaimedDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckOutcomeEffectiveDateIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var outcomePatch = new Models.OutcomesPatch { OutcomeEffectiveDate = DateTime.MaxValue };
            var json = JsonConvert.SerializeObject(outcomePatch);

            // Act
            var patchedOutcomes = _outcomePatchService.Patch(json, outcomePatch);
            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);

            // Assert
            Assert.That(outcome.OutcomeEffectiveDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckIsPriorityCustomerIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var outcomePatch = new OutcomesPatch { IsPriorityCustomer = true };
            var json = JsonConvert.SerializeObject(outcomePatch);

            // Act
            var patchedOutcomes = _outcomePatchService.Patch(json, outcomePatch);
            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);

            // Assert
            Assert.That(outcome.IsPriorityCustomer, Is.True);
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckTouchpointId_WhenPatchIsCalled()
        {
            // Arrange
            var outcomePatch = new OutcomesPatch { TouchpointId = "0000000111" };
            var json = JsonConvert.SerializeObject(outcomePatch);

            // Act
            var patchedOutcomes = _outcomePatchService.Patch(json, outcomePatch);
            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);

            // Assert
            Assert.That(outcome.TouchpointId, Is.EqualTo("0000000111"));
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var outcomePatch = new OutcomesPatch { LastModifiedDate = DateTime.MaxValue };
            var json = JsonConvert.SerializeObject(outcomePatch);

            // Act
            var patchedOutcomes = _outcomePatchService.Patch(json, outcomePatch);
            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);

            // Assert
            Assert.That(outcome.LastModifiedDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void OutcomesPatchServiceTests_CheckLastModifiedByUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var outcomePatch = new OutcomesPatch { LastModifiedTouchpointId = "0000000111" };
            var json = JsonConvert.SerializeObject(outcomePatch);

            // Act
            var patchedOutcomes = _outcomePatchService.Patch(json, outcomePatch);
            var outcome = JsonConvert.DeserializeObject<Models.Outcomes>(patchedOutcomes);

            // Assert
            Assert.That(outcome.LastModifiedTouchpointId, Is.EqualTo("0000000111"));
        }
        
    }
}