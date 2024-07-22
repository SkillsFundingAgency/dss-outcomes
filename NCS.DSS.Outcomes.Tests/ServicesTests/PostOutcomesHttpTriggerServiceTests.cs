using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.Tests.ServicesTests
{
    [TestFixture]
    public class PostOutcomesHttpTriggerServiceTests
    {
        private IPostOutcomesHttpTriggerService _postOutcomesHttpTriggerService;
        private Mock<IDocumentDBProvider> _documentDbProvider;
        private Models.Outcomes _outcome;

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            _postOutcomesHttpTriggerService = new PostOutcomesHttpTriggerService(_documentDbProvider.Object);
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

            _documentDbProvider.Setup(x=>x.CreateOutcomesAsync(It.IsAny<Models.Outcomes>())).Returns(Task.FromResult(resourceResponse));

            // Act
            var result = await _postOutcomesHttpTriggerService.CreateAsync(_outcome);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Outcomes>());

        }
    }
}
