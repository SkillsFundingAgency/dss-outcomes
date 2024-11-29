using Microsoft.Azure.Cosmos;
using Moq;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.Models;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.ServiceBus;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.Tests.ServicesTests
{
    [TestFixture]
    public class PatchOutcomesHttpTriggerServiceTests
    {
        private IPatchOutcomesHttpTriggerService _outcomePatchHttpTriggerService;
        private Mock<IOutcomePatchService> _outcomePatchService;
        private Mock<ICosmosDBProvider> _cosmosDbProvider;
        private Mock<IOutcomesServiceBusClient> _outcomesServiceBusClient;

        private readonly Guid _outcomeId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _outcomePatchService = new Mock<IOutcomePatchService>();
            _outcomesServiceBusClient = new Mock<IOutcomesServiceBusClient>();
            _outcomePatchHttpTriggerService = new PatchOutcomesHttpTriggerService(_cosmosDbProvider.Object, _outcomePatchService.Object, _outcomesServiceBusClient.Object);
        }


        [Test]
        public void PatchOutcomesHttpTriggerServiceTests_PatchResource_ReturnsNullWhenOutcomeJsonIsNullOrEmpty()
        {
            // Act
            var result = _outcomePatchHttpTriggerService.PatchResource(null, Arg.Any<OutcomesPatch>());

            // Assert
            Assert.That(result, Is.Null);
        }


        [Test]
        public void PatchOutcomesHttpTriggerServiceTests_PatchResource_ReturnsNullWhenOutcomePatchIsNullOrEmpty()
        {
            // Act
            var result = _outcomePatchHttpTriggerService.PatchResource(Arg.Any<string>(), null);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchOutcomesHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenOutcomePatchServicePatchJsonIsNullOrEmpty()
        {
            // Arrange
            var outcomePatch = new Models.OutcomesPatch { OutcomeEffectiveDate = DateTime.MaxValue };
            var json = JsonConvert.SerializeObject(outcomePatch);
            _outcomePatchService.Setup(x => x.Patch(It.IsAny<string>(), It.IsAny<OutcomesPatch>())).Returns(json);

            // Act
            var result = await _outcomePatchHttpTriggerService.UpdateCosmosAsync(json, _outcomeId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchOutcomesHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeUpdated()
        {
            // Arrange
            var outcomePatch = new Models.OutcomesPatch { OutcomeEffectiveDate = DateTime.MaxValue };
            var json = JsonConvert.SerializeObject(outcomePatch);

            // Act
            var result = await _outcomePatchHttpTriggerService.UpdateCosmosAsync(json, _outcomeId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchOutcomesHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            // Arrange
            var outcomePatch = new OutcomesPatch { OutcomeEffectiveDate = DateTime.MaxValue };
            var json = JsonConvert.SerializeObject(outcomePatch);
            _cosmosDbProvider.Setup(x => x.CreateOutcomesAsync(It.IsAny<Models.Outcomes>())).Returns(Task.FromResult(new Mock<ItemResponse<Models.Outcomes>>().Object));

            // Act
            var result = await _outcomePatchHttpTriggerService.UpdateCosmosAsync(json, _outcomeId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchOutcomesHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsResourceWhenUpdated()
        {
            var outcomePatch = new OutcomesPatch { OutcomeEffectiveDate = DateTime.MaxValue };
            var json = JsonConvert.SerializeObject(outcomePatch);

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
            .Returns(HttpStatusCode.OK);

            var resourceResponse = mockItemResponse.Object;


            _cosmosDbProvider.Setup(x => x.UpdateOutcomesAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(resourceResponse));

            // Act
            var result = await _outcomePatchHttpTriggerService.UpdateCosmosAsync(json, _outcomeId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Outcomes>());
        }

        [Test]
        public async Task PatchOutcomesHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            // Arrange
            _cosmosDbProvider.Setup(x => x.GetOutcomesForCustomerAsyncToUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(null));

            // Act
            var result = await _outcomePatchHttpTriggerService.GetOutcomesForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchOutcomesHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            var outcomePatch = new Models.OutcomesPatch { OutcomeEffectiveDate = DateTime.MaxValue };
            var json = JsonConvert.SerializeObject(outcomePatch);
            _cosmosDbProvider.Setup(x => x.GetOutcomesForCustomerAsyncToUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(json));

            // Act
            var result = await _outcomePatchHttpTriggerService.GetOutcomesForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>());

            // Assert
            Assert.That(result, Is.InstanceOf<string>());
            Assert.That(result, Is.Not.Null);
        }
    }
}
