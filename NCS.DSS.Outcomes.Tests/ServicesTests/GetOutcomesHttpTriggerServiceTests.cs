using Moq;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.Tests.ServicesTests
{
    [TestFixture]
    public class GetOutcomesHttpTriggerServiceTests
    {
        private IGetOutcomesHttpTriggerService _outcomeHttpTriggerService;
        private Mock<IDocumentDBProvider> _documentDbProvider;
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            _outcomeHttpTriggerService = new GetOutcomesHttpTriggerService(_documentDbProvider.Object);
        }

        [Test]
        public async Task GetOutcomesByIdHttpTriggerServiceTests_GetOutcomesForCustomerAsyncc_ReturnsNullWhenResourceCannotBeFound()
        {
            // Arrange
            _documentDbProvider.Setup(x=>x.GetOutcomesForCustomerAsync(It.IsAny<Guid>())).Returns(Task.FromResult<List<Models.Outcomes>>(null));

            // Act
            var result = await _outcomeHttpTriggerService.GetOutcomesAsync(_customerId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetOutcomesByIdHttpTriggerServiceTests_GetOutcomesForCustomerAsync_ReturnsResource()
        {
            // Arrange
            _documentDbProvider.Setup(x=>x.GetOutcomesForCustomerAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new List<Models.Outcomes>()));

            // Act
            var result = await _outcomeHttpTriggerService.GetOutcomesAsync(_customerId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<List<Models.Outcomes>>());
        }
    }
}
