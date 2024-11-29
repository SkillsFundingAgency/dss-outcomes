using Microsoft.Azure.Cosmos;
using Moq;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.ServiceBus;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.Tests.ServicesTests
{
    [TestFixture]
    public class PostOutcomesHttpTriggerServiceTests
    {
        private IPostOutcomesHttpTriggerService _postOutcomesHttpTriggerService;
        private Mock<ICosmosDBProvider> _cosmosDbProvider;
        private Models.Outcomes _outcome;
        private Mock<IOutcomesServiceBusClient> _outcomesServiceBusClient;

        [SetUp]
        public void Setup()
        {
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _outcomesServiceBusClient = new Mock<IOutcomesServiceBusClient>();
            _postOutcomesHttpTriggerService = new PostOutcomesHttpTriggerService(_cosmosDbProvider.Object, _outcomesServiceBusClient.Object);
            _outcome = Substitute.For<Models.Outcomes>();
        }

        [Test]
        public async Task PostOutcomesHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenOutcomeJsonIsNullOrEmpty()
        {
            // Arrange

            // Act
            var result = await _postOutcomesHttpTriggerService.CreateAsync(null);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PostOutcomesHttpTriggerServiceTests_CreateAsync_ReturnsResourceWhenUpdated()
        {
            // Arrange
            var mockOutcome = new Models.Outcomes
            {
                OutcomeId = new Guid("9c0d182f-5d62-4b64-921e-ab80d6352c57"),
                CustomerId = new Guid("8840cb20-2436-431b-9e93-5899bb6ea966"),
                ActionPlanId = new Guid("22b49d9f-f6eb-4aff-919e-e1dc7f413db7"),
                SessionId = new Guid("cce61da8-b7a8-4843-b308-39c8c380210e"),
                SubcontractorId = "12345678",
                OutcomeType = ReferenceData.OutcomeType.CareersManagement,
                OutcomeClaimedDate = null,
                OutcomeEffectiveDate = null,
                IsPriorityCustomer = false,
                TouchpointId = "9999999999",
                LastModifiedTouchpointId = "9999999999"
            };

            var mockItemResponse = new Mock<ItemResponse<Models.Outcomes>>();

            mockItemResponse
            .Setup(response => response.Resource)
            .Returns(mockOutcome);
            mockItemResponse
            .Setup(response => response.StatusCode)
            .Returns(HttpStatusCode.Created);

            var resourceResponse = mockItemResponse.Object;

            _cosmosDbProvider.Setup(x => x.CreateOutcomesAsync(It.IsAny<Models.Outcomes>())).Returns(Task.FromResult(resourceResponse));

            // Act
            var result = await _postOutcomesHttpTriggerService.CreateAsync(_outcome);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Outcomes>());
        }
    }
}
