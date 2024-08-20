using NCS.DSS.Outcomes.Validation;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Outcomes.Tests.ValidationTests
{
    [TestFixture]
    public class ValidateTests_Patch
    {
        private IValidate _validate;

        [SetUp]
        public void Setup()
        {
            _validate = new Validate();
        }


        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedTouchpointIdIsValid()
        {
            var outcomes = new Models.OutcomesPatch
            {
                SessionId = Guid.NewGuid(),
                LastModifiedTouchpointId = "0000000001"
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count.Equals(0));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedTouchpointIdIsInvalid()
        {
            var outcomes = new Models.OutcomesPatch
            {
                SessionId = Guid.NewGuid(),
                LastModifiedTouchpointId = "000000000A"
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count.Equals(1));
        }


        [Test]
        public void ValidateTests_ReturnValidationResult_WhenTouchpointIdIsValid()
        {
            var outcomes = new Models.OutcomesPatch
            {
                SessionId = Guid.NewGuid(),
                TouchpointId = "0000000001"
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count.Equals(0));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenTouchpointIdIsInvalid()
        {
            var outcomes = new Models.OutcomesPatch
            {
                SessionId = Guid.NewGuid(),
                TouchpointId = "000000000A"
            };

            var result = _validate.ValidateResource(outcomes, Arg.Any<DateTime>());

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count.Equals(1));
        }
    }
}
