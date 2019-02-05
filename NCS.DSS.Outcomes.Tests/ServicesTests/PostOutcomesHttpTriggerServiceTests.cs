using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Outcomes.Tests.ServicesTests
{
    [TestFixture]
    public class PostOutcomesHttpTriggerServiceTests
    {
        private IPostOutcomesHttpTriggerService _postOutcomesHttpTriggerService;
        private IDocumentDBProvider _documentDbProvider;
        private Models.Outcomes _outcome;

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _postOutcomesHttpTriggerService = Substitute.For<PostOutcomesHttpTriggerService>(_documentDbProvider);
            _outcome = Substitute.For<Models.Outcomes>();
        }

        [Test]
        public async Task PostOutcomesHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenOutcomeJsonIsNullOrEmpty()
        {
            // Act
            var result = await _postOutcomesHttpTriggerService.CreateAsync(null);

            // Assert
            Assert.IsNull(result);
        }
        
        [Test]
        public async Task PostOutcomesHttpTriggerServiceTests_CreateAsync_ReturnsResourceWhenUpdated()
        {
            const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            var resourceResponse = new ResourceResponse<Document>(new Document());
            var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.Created, null };

            var documentServiceResponse = documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            _documentDbProvider.CreateOutcomesAsync(Arg.Any<Models.Outcomes>()).Returns(Task.FromResult(resourceResponse).Result);

            // Act
            var result = await _postOutcomesHttpTriggerService.CreateAsync(_outcome);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.Outcomes>(result);

        }
    }
}
