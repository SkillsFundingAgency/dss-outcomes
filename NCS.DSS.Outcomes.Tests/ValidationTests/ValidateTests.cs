﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Outcomes.ReferenceData;
using NCS.DSS.Outcomes.Validation;
using NUnit.Framework;

namespace NCS.DSS.Outcomes.Tests.ValidationTests
{
    [TestFixture]
    public class ValidateTests
    {

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenOutcomesIsNotSuppliedForPost()
        {
            var outcomes = new Models.Outcomes();

            var validation = new Validate();

            var result = validation.ValidateResource(outcomes);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenOutcomeClaimedDateIsNotSuppliedForPost()
        {
            var outcomes = new Models.Outcomes
            {
                OutcomeEffectiveDate = DateTime.UtcNow
            };

            var validation = new Validate();

            var result = validation.ValidateResource(outcomes);

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
            };

            var validation = new Validate();

            var result = validation.ValidateResource(outcomes);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenOutcomeClaimedDateIsInTheFuture()
        {
            var outcomes = new Models.Outcomes
            {
                OutcomeClaimedDate = DateTime.MaxValue,
                OutcomeEffectiveDate = DateTime.UtcNow
            };

            var validation = new Validate();

            var result = validation.ValidateResource(outcomes);

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
                OutcomeEffectiveDate = DateTime.MaxValue
            };

            var validation = new Validate();

            var result = validation.ValidateResource(outcomes);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedDateIsInTheFuture()
        {
            var outcomes = new Models.Outcomes
            {
                OutcomeClaimedDate = DateTime.UtcNow,
                OutcomeEffectiveDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.MaxValue
            };

            var validation = new Validate();

            var result = validation.ValidateResource(outcomes);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenOutcomeTypeIsNotValid()
        {
            var outcomes = new Models.Outcomes
            {
                OutcomeClaimedDate = DateTime.UtcNow,
                OutcomeEffectiveDate = DateTime.UtcNow,
                OutcomeType = (OutcomeType)100
            };

            var validation = new Validate();

            var result = validation.ValidateResource(outcomes);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

    }
}