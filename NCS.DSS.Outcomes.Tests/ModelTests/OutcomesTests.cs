using System;
using NSubstitute;
using NUnit.Framework;

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
            Assert.IsNotNull(diversity.LastModifiedDate);
        }

        [Test]
        public void OutcomesTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var diversity = new Models.Outcomes { LastModifiedDate = DateTime.MaxValue };

            diversity.SetDefaultValues();

            // Assert
            Assert.AreEqual(DateTime.MaxValue, diversity.LastModifiedDate);
        }

        [Test]
        public void OutcomesTests_CheckOutcomesIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Outcomes();

            diversity.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreNotSame(Guid.Empty, diversity.OutcomeId);
        }

        [Test]
        public void OutcomesTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Outcomes();

            var customerId = Guid.NewGuid();
            diversity.SetIds(customerId, Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreEqual(customerId, diversity.CustomerId);
        }

        [Test]
        public void OutcomesTests_CheckActionPlanIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Outcomes();

            var actionPlanId = Guid.NewGuid();
            diversity.SetIds(Arg.Any<Guid>(), actionPlanId, Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreEqual(actionPlanId, diversity.ActionPlanId);
        }

        [Test]
        public void OutcomesTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Outcomes();

            diversity.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), "0000000000", Arg.Any<string>());

            // Assert
            Assert.AreEqual("0000000000", diversity.LastModifiedTouchpointId);
        }

        [Test]
        public void OutcomesTests_CheckSubcontractorIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.Outcomes();

            diversity.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(),  Arg.Any<string>(), "0000000000");

            // Assert
            Assert.AreEqual("0000000000", diversity.SubcontractorId);
        }

    }
}
