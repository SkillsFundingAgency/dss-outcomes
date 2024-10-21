using NSubstitute;
using NUnit.Framework;
using System;

namespace NCS.DSS.Outcomes.Tests.ModelTests
{

    [TestFixture]
    public class OutcomesTests
    {

        [Test]
        public void OutcomesTests_PopulatesDefaultValues_WhenSetDefaultValuesIsCalled()
        {
            var diversity = new Models.Outcomes();
            diversity.SetDefaultValues();

            // Assert
            Assert.That(diversity.IsPriorityCustomer, Is.False);
            Assert.That(diversity.LastModifiedDate, Is.Not.Null);
        }

        [Test]
        public void OutcomesTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var diversity = new Models.Outcomes { LastModifiedDate = DateTime.MaxValue };

            diversity.SetDefaultValues();

            // Assert
            Assert.That(diversity.LastModifiedDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void OutcomesTests_CheckOutcomesIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Outcomes();

            diversity.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.That(diversity.OutcomeId, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void OutcomesTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Outcomes();

            var customerId = Guid.NewGuid();
            diversity.SetIds(customerId, Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.That(diversity.CustomerId, Is.EqualTo(customerId));
        }

        [Test]
        public void OutcomesTests_CheckActionPlanIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Outcomes();

            var actionPlanId = Guid.NewGuid();
            diversity.SetIds(Arg.Any<Guid>(), actionPlanId, Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.That(diversity.ActionPlanId, Is.EqualTo(actionPlanId));
        }

        [Test]
        public void OutcomesTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Outcomes();

            diversity.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), "0000000000", Arg.Any<string>());

            // Assert
            Assert.That(diversity.LastModifiedTouchpointId, Is.EqualTo("0000000000"));
        }

        [Test]
        public void OutcomesTests_CheckSubcontractorIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Outcomes();

            diversity.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), "0000000000");

            // Assert
            Assert.That(diversity.SubcontractorId, Is.EqualTo("0000000000"));
        }

    }
}
