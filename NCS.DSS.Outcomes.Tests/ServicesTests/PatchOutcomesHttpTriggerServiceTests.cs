using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.Models;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace NCS.DSS.Outcomes.Tests.ServicesTests
{
    [TestFixture]
    public class PatchOutcomesHttpTriggerServiceTests
    {
        private IPatchOutcomesHttpTriggerService _outcomePatchHttpTriggerService;
        private IOutcomePatchService _outcomePatchService;
        private IDocumentDBProvider _documentDbProvider;
        private Models.Outcomes _outcome;
        private OutcomesPatch _outcomePatch;
        private string _json;

        private readonly Guid _outcomeId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private readonly Guid _interactionId = Guid.Parse("a06b29da-d949-4486-8b18-c0107dc8bae8");
        private readonly Guid _actionPlanId = Guid.Parse("679897e6-c16a-41ba-90c9-f4fbd0a9f666");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _outcomePatchService = Substitute.For<IOutcomePatchService>();
            _outcomePatchHttpTriggerService = Substitute.For<PatchOutcomesHttpTriggerService>(_documentDbProvider, _outcomePatchService);
            _outcome = Substitute.For<Models.Outcomes>();
            _outcomePatch = Substitute.For<OutcomesPatch>();
            _json = JsonConvert.SerializeObject(_outcomePatch);
            _outcomePatchService.Patch(_json, _outcomePatch).Returns(_outcome);
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
            _outcomePatchService.Patch(Arg.Any<string>(), Arg.Any<OutcomesPatch>()).ReturnsNull();

            // Act
            var result = await _outcomePatchHttpTriggerService.UpdateCosmosAsync(_outcome);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task  PatchOutcomesHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeUpdated()
        {
            _documentDbProvider.UpdateOutcomesAsync(Arg.Any<Models.Outcomes>()).ReturnsNull();

            // Act
            var result = await _outcomePatchHttpTriggerService.UpdateCosmosAsync(_outcome);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task  PatchOutcomesHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.CreateOutcomesAsync(Arg.Any<Models.Outcomes>()).Returns(Task.FromResult(new ResourceResponse<Document>(null)).Result);

            // Act
            var result = await _outcomePatchHttpTriggerService.UpdateCosmosAsync(_outcome);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task  PatchOutcomesHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsResourceWhenUpdated()
        {
            const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            var resourceResponse = new ResourceResponse<Document>(new Document());
            var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.OK, null };

            var documentServiceResponse = documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            _documentDbProvider.UpdateOutcomesAsync(Arg.Any<Models.Outcomes>()).Returns(Task.FromResult(resourceResponse).Result);

            // Act
            var result = await _outcomePatchHttpTriggerService.UpdateCosmosAsync(_outcome);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.Outcomes>(result);

        }

        [Test]
        public async Task  PatchOutcomesHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            _documentDbProvider.GetOutcomesForCustomerAsyncToUpdateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).ReturnsNull();

            // Act
            var result = await _outcomePatchHttpTriggerService.GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task  PatchOutcomesHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            _documentDbProvider.GetOutcomesForCustomerAsyncToUpdateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_json).Result);

            // Act
            var result = await _outcomePatchHttpTriggerService.GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<string>(result);
        }
    }
}
