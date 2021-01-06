using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.Models;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace NCS.DSS.Outcomes.Tests.ServicesTests
{
    [TestFixture]
    public class PatchOutcomesHttpTriggerServiceTests
    {
        private IPatchOutcomesHttpTriggerService _outcomePatchHttpTriggerService;
        private Mock<IOutcomePatchService> _outcomePatchService;
        private Mock<IDocumentDBProvider> _documentDbProvider;

        private readonly Guid _outcomeId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            _outcomePatchService = new Mock<IOutcomePatchService>();
            _outcomePatchHttpTriggerService = new PatchOutcomesHttpTriggerService(_documentDbProvider.Object, _outcomePatchService.Object);
        }


        [Test]
        public void PatchOutcomesHttpTriggerServiceTests_PatchResource_ReturnsNullWhenOutcomeJsonIsNullOrEmpty()
        {
            // Act
            var result = _outcomePatchHttpTriggerService.PatchResource(null, Arg.Any<OutcomesPatch>());

            // Assert
            Assert.IsNull(result);
        }


        [Test]
        public void PatchOutcomesHttpTriggerServiceTests_PatchResource_ReturnsNullWhenOutcomePatchIsNullOrEmpty()
        {
            // Act
            var result = _outcomePatchHttpTriggerService.PatchResource(Arg.Any<string>(), null);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task  PatchOutcomesHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenOutcomePatchServicePatchJsonIsNullOrEmpty()
        {
            // Arrange
            var outcomePatch = new Models.OutcomesPatch { OutcomeEffectiveDate = DateTime.MaxValue };
            var json = JsonConvert.SerializeObject(outcomePatch);
            _outcomePatchService.Setup(x => x.Patch(It.IsAny<string>(), It.IsAny<OutcomesPatch>())).Returns(json);

            // Act
            var result = await _outcomePatchHttpTriggerService.UpdateCosmosAsync(json, _outcomeId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task  PatchOutcomesHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeUpdated()
        {
            // Arrange
            var outcomePatch = new Models.OutcomesPatch { OutcomeEffectiveDate = DateTime.MaxValue };
            var json = JsonConvert.SerializeObject(outcomePatch);

            // Act
            var result = await _outcomePatchHttpTriggerService.UpdateCosmosAsync(json, _outcomeId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task  PatchOutcomesHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            // Arrange
            var outcomePatch = new Models.OutcomesPatch { OutcomeEffectiveDate = DateTime.MaxValue };
            var json = JsonConvert.SerializeObject(outcomePatch);
            _documentDbProvider.Setup(x=>x.CreateOutcomesAsync(It.IsAny<Models.Outcomes>())).Returns(Task.FromResult(new ResourceResponse<Document>(null)));

            // Act
            var result = await _outcomePatchHttpTriggerService.UpdateCosmosAsync(json, _outcomeId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task  PatchOutcomesHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsResourceWhenUpdated()
        {
            //var _outcome = Substitute.For<Models.Outcomes>();
            //const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            //const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            //_outcomePatchService.Object.Patch(_json, _outcomePatch).Returns(_outcome.ToString());
            //var resourceResponse = new ResourceResponse<Document>(new Document());
            //var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            //const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            //var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            //var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            //var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            //var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.OK, null };

            //var documentServiceResponse = documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            //var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            //responseField?.SetValue(resourceResponse, documentServiceResponse);

            //_documentDbProvider.Object.UpdateOutcomesAsync(It.IsAny<string>(), It.IsAny<Guid>()).Returns(Task.FromResult(resourceResponse).Result);

            //// Act
            //var result = await _outcomePatchHttpTriggerService.UpdateCosmosAsync(_outcome.ToString(), _outcomeId);

            //// Assert
            //Assert.IsNotNull(result);
            //Assert.IsInstanceOf<Models.Outcomes>(result);

        }

        [Test]
        public async Task  PatchOutcomesHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            // Arrange
            _documentDbProvider.Setup(x => x.GetOutcomesForCustomerAsyncToUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(null));

            // Act
            var result = await _outcomePatchHttpTriggerService.GetOutcomesForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>());

            // Assert
            Assert.Null(result);
        }

        [Test]
        public async Task  PatchOutcomesHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            var outcomePatch = new Models.OutcomesPatch { OutcomeEffectiveDate = DateTime.MaxValue };
            var json = JsonConvert.SerializeObject(outcomePatch);
            _documentDbProvider.Setup(x=>x.GetOutcomesForCustomerAsyncToUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(json));

            // Act
            var result = await _outcomePatchHttpTriggerService.GetOutcomesForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<string>(result);
        }
    }
}
