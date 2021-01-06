using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.DeleteOutcomesHttpTrigger.Service;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.Tests.FunctionTests
{
    [TestFixture]
    public class DeleteOutcomesHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidActionPlanId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string ValidOutcomeId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string ValidSessionId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private Mock<ILogger> _log;
        private HttpRequest _request;
        private Mock<IDeleteOutcomesHttpTriggerService> _deleteOutcomesHttpTriggerService;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<ILoggerHelper> _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;

        [SetUp]
        public void Setup()
        {
            _resourceHelper = new Mock<IResourceHelper>();
            _loggerHelper = new Mock<ILoggerHelper>();
            _httpRequestHelper = new HttpRequestHelper();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();
            _log = new Mock<ILogger>();
            _deleteOutcomesHttpTriggerService = new Mock<IDeleteOutcomesHttpTriggerService>();
        }

        [Test]
        public async Task DeleteOutcomesHttpTriggerTests_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task DeleteOutcomesHttpTriggerTests_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Arrange

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task DeleteOutcomesHttpTriggerTests_ReturnsStatusCodeBadRequest_WhenAcionPlanIdIsInvalid()
        {
            // Arrange

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId, ValidOutcomeId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task DeleteOutcomesHttpTriggerTests_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }


        [Test]
        public async Task DeleteOutcomesHttpTriggerTests_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            //Arrange
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x=>x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task DeleteOutcomesHttpTriggerTests_ReturnsStatusCodeNoContent_WhenActionPlanDoesNotExist()
        {
            // Arrange
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x=>x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x=>x.DoesActionPlanResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);
            _deleteOutcomesHttpTriggerService.Setup(x=>x.GetOutcomeForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Outcomes>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }


        [Test]
        public async Task DeleteOutcomesHttpTriggerTests_ReturnsStatusCodeOk_WhenOutcomesDoesNotExist()
        {
            // Arrange
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x=>x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x=>x.DoesActionPlanResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _deleteOutcomesHttpTriggerService.Setup(x=>x.GetOutcomeForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Outcomes>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task DeleteOutcomesHttpTriggerTests_ReturnsStatusCodeBadRequest_WhenRequestIsNotValid()
        {
            // Arrange
            var outcome = new Models.Outcomes() { CustomerId = new Guid(ValidCustomerId) };
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x=>x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x=>x.DoesActionPlanResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _deleteOutcomesHttpTriggerService.Setup(x=>x.GetOutcomeForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(outcome));
            _deleteOutcomesHttpTriggerService.Setup(x=>x.DeleteAsync(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task DeleteOutcomesHttpTriggerTests_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            // Arrange
            var outcome = new Models.Outcomes() { CustomerId = new Guid(ValidCustomerId) };
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x=>x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x=>x.DoesActionPlanResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _deleteOutcomesHttpTriggerService.Setup(x=>x.GetOutcomeForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(outcome));
            _deleteOutcomesHttpTriggerService.Setup(x=>x.DeleteAsync(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string actionPlanId, string outcomeId)
        {
            return await DeleteOutcomesHttpTrigger.Function.DeleteOutcomesHttpTrigger.Run(
                _request,
                _log.Object,
                customerId,
                interactionId,
                actionPlanId,
                outcomeId,
                _resourceHelper.Object,
                _deleteOutcomesHttpTriggerService.Object,
                _loggerHelper.Object,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                _jsonHelper).ConfigureAwait(false);
        }

    }
}