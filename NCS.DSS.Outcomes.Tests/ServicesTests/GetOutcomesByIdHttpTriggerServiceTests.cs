using System;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Outcomes.Tests.ServicesTests
{
    [TestFixture]
    public class GetOutcomesByIdHttpTriggerServiceTests
    {
        private IGetOutcomesByIdHttpTriggerService _outcomeHttpTriggerService;
        private IDocumentDBProvider _documentDbProvider;
        private Models.Outcomes _outcome;
        private readonly Guid _outcomeId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private readonly Guid _interactionId = Guid.Parse("a06b29da-d949-4486-8b18-c0107dc8bae8");
        private readonly Guid _actionPlanId = Guid.Parse("679897e6-c16a-41ba-90c9-f4fbd0a9f666");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _outcomeHttpTriggerService = Substitute.For<GetOutcomesByIdHttpTriggerService>(_documentDbProvider);
            _outcome = Substitute.For<Models.Outcomes>();
        }

        [Test]
        public async Task GetOutcomesByIdHttpTriggerServiceTests_GetOutcomesForCustomerAsyncc_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Outcomes>(null).Result);

            // Act
            var result = await _outcomeHttpTriggerService.GetOutcomesForCustomerAsync(_customerId, _interactionId, _actionPlanId, _outcomeId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetOutcomesByIdHttpTriggerServiceTests_GetOutcomesForCustomerAsync_ReturnsResource()
        {
            _documentDbProvider.GetOutcomesForCustomerAsync(_customerId, _interactionId, _actionPlanId, _outcomeId).Returns(Task.FromResult(_outcome).Result);

            // Act
            var result = await _outcomeHttpTriggerService.GetOutcomesForCustomerAsync(_customerId, _interactionId, _actionPlanId, _outcomeId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.Outcomes>(result);
        }
    }
}
