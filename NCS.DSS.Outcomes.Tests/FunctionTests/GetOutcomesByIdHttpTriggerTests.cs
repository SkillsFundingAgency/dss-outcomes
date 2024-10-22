using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.Tests.FunctionTests
{
    [TestFixture]
    public class GetOutcomesByIdHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidActionPlanId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string ValidOutcomeId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string ValidSessionId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private ILogger<GetOutcomesByIdHttpTrigger.Function.GetOutcomesByIdHttpTrigger> _log;
        private HttpRequest _request;
        private IResourceHelper _resourceHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IGetOutcomesByIdHttpTriggerService _getOutcomesByIdHttpTriggerService;
        private Models.Outcomes _outcome;
        private GetOutcomesByIdHttpTrigger.Function.GetOutcomesByIdHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _outcome = Substitute.For<Models.Outcomes>();

            _request = new DefaultHttpContext().Request;

            _log = Substitute.For<ILogger<GetOutcomesByIdHttpTrigger.Function.GetOutcomesByIdHttpTrigger>>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _resourceHelper = Substitute.For<IResourceHelper>();

            _getOutcomesByIdHttpTriggerService = Substitute.For<IGetOutcomesByIdHttpTriggerService>();
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssSubcontractorId(_request).Returns("9999999999");
            _function = new GetOutcomesByIdHttpTrigger.Function.GetOutcomesByIdHttpTrigger(
                _resourceHelper,
                _httpRequestHelper,
                _getOutcomesByIdHttpTriggerService,
                _log
            );
        }

        [Test]
        public async Task GetOutcomesByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenTSubcontractorIdIsNotProvided()
        {
            _httpRequestHelper.GetDssSubcontractorId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetOutcomesByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetOutcomesByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetOutcomesByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenActionPlanIdIsInvalid()
        {

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetOutcomesByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenOutcomeIdIsInvalid()
        {

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetOutcomesByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetOutcomesByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            _resourceHelper.DoesInteractionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetOutcomesByIdHttpTrigger_ReturnsStatusCodeOk_WhenActionPlanDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetOutcomesByIdHttpTrigger_ReturnsStatusCodeOk_WhenOutcomesDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _getOutcomesByIdHttpTriggerService.GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Outcomes>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetOutcomesByIdHttpTrigger_ReturnsStatusCodeOk_WhenOutcomesExists()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _getOutcomesByIdHttpTriggerService.GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_outcome).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);
            var responseResult = result as JsonResult;

            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId, string actionPlanId, string outcomeId)
        {
            return await _function.RunAsync(
                _request,
                customerId,
                interactionId,
                actionPlanId,
                outcomeId);
        }
    }
}