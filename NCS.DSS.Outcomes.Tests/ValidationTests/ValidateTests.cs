﻿using NCS.DSS.Outcomes.ReferenceData;
using NCS.DSS.Outcomes.Validation;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Outcomes.Tests.ValidationTests
{
    [TestFixture]
    public class ValidateTests
    {
        private const string ValidSubContratorId = "12345678";
        private const string InValidSubContractorId = "123";
        private IValidate _validate;

        [SetUp]
        public void Setup()
        {
            _validate = new Validate();
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenOutcomesIsNotSuppliedForPost()
        {
            var outcomes = new Models.Outcomes();

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenOutcomeClaimedDateIsNotSuppliedForPost()
        {
            var outcomes = new Models.Outcomes
            {
                OutcomeEffectiveDate = DateTime.UtcNow,
                SessionId = Guid.Empty,
                SubcontractorId = ValidSubContratorId
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenOutcomeEffectiveDateIsNotSuppliedForPost()
        {
            var outcomes = new Models.Outcomes
            {
                OutcomeClaimedDate = DateTime.UtcNow,
                SessionId = Guid.Empty,
                SubcontractorId = ValidSubContratorId
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenOutcomeClaimedDateIsInTheFuture()
        {
            var outcomes = new Models.Outcomes
            {
                OutcomeClaimedDate = DateTime.MaxValue,
                OutcomeEffectiveDate = DateTime.UtcNow,
                SessionId = Guid.Empty,
                SubcontractorId = ValidSubContratorId
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenOutcomeEffectiveDateIsInTheFuture()
        {
            var outcomes = new Models.Outcomes
            {
                OutcomeClaimedDate = DateTime.UtcNow,
                OutcomeEffectiveDate = DateTime.MaxValue,
                SessionId = Guid.Empty,
                SubcontractorId = ValidSubContratorId
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedDateIsInTheFuture()
        {
            var outcomes = new Models.Outcomes
            {
                OutcomeClaimedDate = DateTime.UtcNow,
                OutcomeEffectiveDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.MaxValue,
                SessionId = Guid.Empty,
                SubcontractorId = ValidSubContratorId
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenOutcomeTypeIsNotValid()
        {
            var outcomes = new Models.Outcomes
            {
                OutcomeClaimedDate = DateTime.UtcNow,
                OutcomeEffectiveDate = DateTime.UtcNow,
                OutcomeType = (OutcomeType)100,
                SessionId = Guid.Empty,
                SubcontractorId = ValidSubContratorId
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenClaimedPriorityGroupIdDoesntHaveAValueAndOutcomeClaimedDateHasAValue()
        {
            var outcomes = new Models.Outcomes
            {
                OutcomeClaimedDate = DateTime.UtcNow,
                OutcomeEffectiveDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.MaxValue,
                SessionId = Guid.Empty,
                SubcontractorId = ValidSubContratorId
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenSubcontractorIdNotValid()
        {
            var outcomes = new Models.Outcomes
            {
                SubcontractorId = ValidSubContratorId
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenSubcontractorIdValid()
        {
            var outcomes = new Models.Outcomes
            {
                SubcontractorId = InValidSubContractorId
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedTouchpointIdIsValidForPost()
        {
            var outcomes = new Models.Outcomes
            {
                ActionPlanId = Guid.NewGuid(),
                SessionId = Guid.NewGuid(),
                SubcontractorId = "01234567",
                OutcomeType = OutcomeType.CareersManagement,
                LastModifiedTouchpointId = "0000000001"
            };

            var result = _validate.ValidateResource(outcomes , Arg.Any<DateTime>());

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.That(result.Count.Equals(0));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedTouchpointIdIsInvalidForPost()
        {
            var outcomes = new Models.Outcomes
            {
                ActionPlanId = Guid.NewGuid(),
                SessionId = Guid.NewGuid(),
                SubcontractorId = "01234567",
                OutcomeType = OutcomeType.CareersManagement,
                LastModifiedTouchpointId = "000000000A"
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.That(result.Count.Equals(1));
        }


        [Test]
        public void ValidateTests_ReturnValidationResult_WhenTouchpointIdIsValidForPost()
        {
            var outcomes = new Models.Outcomes
            {
                ActionPlanId = Guid.NewGuid(),
                SessionId= Guid.NewGuid(),
                SubcontractorId = "01234567",
                OutcomeType = OutcomeType.CareersManagement,
                TouchpointId = "0000000001"
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.That(result.Count.Equals(0));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenTouchpointIdIsInvalidForPost()
        {
            var outcomes = new Models.Outcomes
            {
                ActionPlanId = Guid.NewGuid(),
                SessionId = Guid.NewGuid(),
                SubcontractorId = "01234567",
                OutcomeType = OutcomeType.CareersManagement,
                TouchpointId = "000000000A"
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.That(result.Count.Equals(1));
        }
    }
}