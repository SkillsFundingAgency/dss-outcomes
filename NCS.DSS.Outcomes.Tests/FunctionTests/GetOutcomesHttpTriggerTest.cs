using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.Tests.FunctionTests
{
    [TestFixture]
    public class GetOutcomesHttpTriggerTest
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidActionPlanId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string ValidSessionId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private ILogger<GetOutcomesHttpTrigger.Function.GetOutcomesHttpTrigger> _log;
        private HttpRequest _request;
        private IGetOutcomesHttpTriggerService _getOutcomesHttpTriggerService;
        private IResourceHelper _resourceHelper;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IJsonHelper _jsonHelper;
        private GetOutcomesHttpTrigger.Function.GetOutcomesHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _request = new DefaultHttpContext().Request;

            _loggerHelper = Substitute.For<ILoggerHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _jsonHelper = Substitute.For<IJsonHelper>();
            _log = Substitute.For<ILogger<GetOutcomesHttpTrigger.Function.GetOutcomesHttpTrigger>>();
            _resourceHelper = Substitute.For<IResourceHelper>();

            _getOutcomesHttpTriggerService = Substitute.For<IGetOutcomesHttpTriggerService>();
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssSubcontractorId(_request).Returns("9999999999");
            _function = new GetOutcomesHttpTrigger.Function.GetOutcomesHttpTrigger(
                _resourceHelper,
                _httpRequestHelper,
                _getOutcomesHttpTriggerService,
                _jsonHelper,
                _loggerHelper,
                _log
                );
        }

        [Test]
        public async Task GetOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());

            var badRequestResult = result as BadRequestObjectResult;

            Assert.That(badRequestResult.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task PostOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenTSubcontractorIdIsNotProvided()
        {
            _httpRequestHelper.GetDssSubcontractorId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());

            var badRequestResult = result as BadRequestObjectResult;

            Assert.That(badRequestResult.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());

            var badRequestResult = result as BadRequestObjectResult;

            Assert.That(badRequestResult.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());

            var badRequestResult = result as BadRequestObjectResult;

            Assert.That(badRequestResult.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetOutcomesByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenActionPlanIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());

            var badRequestResult = result as BadRequestObjectResult;

            Assert.That(badRequestResult.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetOutcomesHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());

            var noContentResult = result as NoContentResult;

            Assert.That(noContentResult.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task GetOutcomesHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
             _resourceHelper.DoesInteractionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());

            var noContentResult = result as NoContentResult;

            Assert.That(noContentResult.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task GetOutcomesByIdHttpTrigger_ReturnsStatusCodeOk_WhenActionPlanDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
             _resourceHelper.DoesInteractionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;

            Assert.That(okResult.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetOutcomesHttpTrigger_ReturnsStatusCodeNoContent_WhenOutcomesDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
             _resourceHelper.DoesInteractionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _getOutcomesHttpTriggerService.GetOutcomesAsync(Arg.Any<Guid>()).Returns(Task.FromResult<List<Models.Outcomes>>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());

            var noContentResult = result as NoContentResult;

            Assert.That(noContentResult.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task GetOutcomesHttpTrigger_ReturnsStatusCodeOk_WhenOutcomesExists()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
             _resourceHelper.DoesInteractionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            var listOfOutcomeses = new List<Models.Outcomes>();
            _getOutcomesHttpTriggerService.GetOutcomesAsync(Arg.Any<Guid>()).Returns(Task.FromResult<List<Models.Outcomes>>(listOfOutcomeses).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;

            Assert.That(okResult.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId, string actionPlanId)
        {
            return await _function.RunAsync(
                _request,
                customerId, 
                interactionId, 
                actionPlanId).ConfigureAwait(false);
        }

    }
}