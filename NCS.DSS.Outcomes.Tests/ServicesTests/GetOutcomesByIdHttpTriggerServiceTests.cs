using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.Tests.ServicesTests
{
    [TestFixture]
    public class GetOutcomesByIdHttpTriggerServiceTests
    {
        private IGetOutcomesByIdHttpTriggerService _outcomeHttpTriggerService;
        private Mock<ICosmosDBProvider> _cosmosDbProvider;
        private Mock<ILogger<GetOutcomesByIdHttpTriggerService>> _mockLogger;
        private readonly Guid _outcomeId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private readonly Guid _interactionId = Guid.Parse("a06b29da-d949-4486-8b18-c0107dc8bae8");
        private readonly Guid _actionPlanId = Guid.Parse("679897e6-c16a-41ba-90c9-f4fbd0a9f666");

        [SetUp]
        public void Setup()
        {
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _mockLogger = new Mock<ILogger<GetOutcomesByIdHttpTriggerService>>();
            _outcomeHttpTriggerService = new GetOutcomesByIdHttpTriggerService(_cosmosDbProvider.Object, _mockLogger.Object);
        }

        [Test]
        public async Task GetOutcomesByIdHttpTriggerServiceTests_GetOutcomesForCustomerAsyncc_ReturnsNullWhenResourceCannotBeFound()
        {
            // Arrange
            _cosmosDbProvider.Setup(x => x.GetOutcomeForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Outcomes>(null));

            // Act
            var result = await _outcomeHttpTriggerService.GetOutcomesForCustomerAsync(_customerId, _interactionId, _actionPlanId, _outcomeId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetOutcomesByIdHttpTriggerServiceTests_GetOutcomesForCustomerAsync_ReturnsResource()
        {
            // Arrange
            _cosmosDbProvider.Setup(x => x.GetOutcomeForCustomerAsync(_customerId, _interactionId, _actionPlanId, _outcomeId)).Returns(Task.FromResult(new Models.Outcomes()));

            // Act
            var result = await _outcomeHttpTriggerService.GetOutcomesForCustomerAsync(_customerId, _interactionId, _actionPlanId, _outcomeId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Outcomes>());
        }
    }
}
