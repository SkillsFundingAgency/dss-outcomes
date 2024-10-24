using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.Models;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.Tests.FunctionTests
{
    [TestFixture]
    public class PatchOutcomesHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidActionPlanId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string ValidOutcomeId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string ValidSessionId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private ILogger<PatchOutcomesHttpTrigger.Function.PatchOutcomesHttpTrigger> _log;
        private HttpRequest _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IHttpRequestHelper _httpRequestHelper;
        private IJsonHelper _jsonHelper;
        private IDynamicHelper _dynamicHelper;
        private IPatchOutcomesHttpTriggerService _patchOutcomesHttpTriggerService;
        private Models.Outcomes _outcome;
        private OutcomesPatch _outcomePatch;
        private string _outcomeString;
        private PatchOutcomesHttpTrigger.Function.PatchOutcomesHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _outcome = Substitute.For<Models.Outcomes>();
            _outcomePatch = Substitute.For<OutcomesPatch>();

            _request = new DefaultHttpContext().Request;

            _resourceHelper = Substitute.For<IResourceHelper>();
            _validate = Substitute.For<IValidate>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _jsonHelper = Substitute.For<IJsonHelper>();
            _log = Substitute.For<ILogger<PatchOutcomesHttpTrigger.Function.PatchOutcomesHttpTrigger>>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _dynamicHelper = Substitute.For<IDynamicHelper>();
            _patchOutcomesHttpTriggerService = Substitute.For<IPatchOutcomesHttpTriggerService>();
            _outcomeString = JsonConvert.SerializeObject(_outcome);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.IsCustomerReadOnly().ReturnsForAnyArgs(false);
            _resourceHelper.DoesInteractionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesSessionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.GetDateAndTimeOfSession(Arg.Any<Guid>()).Returns(DateTime.Now);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssSubcontractorId(_request).Returns("9999999999");
            _httpRequestHelper.GetDssApimUrl(_request).Returns("http://localhost:7071/");
            _httpRequestHelper.GetResourceFromRequest<OutcomesPatch>(_request).Returns(Task.FromResult(_outcomePatch).Result);
            _patchOutcomesHttpTriggerService.PatchResource(Arg.Any<string>(), _outcomePatch).Returns(_outcomeString);
            _function = new PatchOutcomesHttpTrigger.Function.PatchOutcomesHttpTrigger(
                _resourceHelper,
                _httpRequestHelper,
                _patchOutcomesHttpTriggerService,
                _jsonHelper,
                _validate,
                _log,
                _dynamicHelper
            );
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
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
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenAcionPlanIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenOutcomeIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeNoContent_WhenOutcomePatchCantBePatched()
        {
            _patchOutcomesHttpTriggerService.GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_outcomeString).Result);

            _patchOutcomesHttpTriggerService.PatchResource(Arg.Any<string>(), Arg.Any<Models.OutcomesPatch>()).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenOutcomesHasFailedValidation()
        {
            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.ValidateResource(Arg.Any<OutcomesPatch>(), Arg.Any<DateTime>()).ReturnsForAnyArgs(validationResults);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }


        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _resourceHelper.DoesInteractionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeNoContent_WhenSessionDoesNotExist()
        {
            _resourceHelper.DoesSessionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns(false);

            _patchOutcomesHttpTriggerService
                .GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns(Task.FromResult<string>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeNoContent_WhenActionPlanDoesNotExist()
        {
            _resourceHelper
                .DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns(false);

            _patchOutcomesHttpTriggerService
                .GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns(Task.FromResult<string>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }


        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeOk_WhenOutcomesDoesNotExist()
        {
            _patchOutcomesHttpTriggerService
                .GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns(Task.FromResult<string>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateOutcomesRecord()
        {
            _patchOutcomesHttpTriggerService
                .GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns(Task.FromResult(_outcomeString).Result);

            _patchOutcomesHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), Arg.Any<Guid>())
                .Returns(Task.FromResult<Models.Outcomes>(null).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenRequestIsNotValid()
        {
            _patchOutcomesHttpTriggerService
                .GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns(Task.FromResult(_outcomeString).Result);

            _patchOutcomesHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), Arg.Any<Guid>())
                .Returns(Task.FromResult<Models.Outcomes>(null).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            _patchOutcomesHttpTriggerService
                .GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns(Task.FromResult(_outcomeString).Result);

            _patchOutcomesHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), Arg.Any<Guid>())
                .Returns(Task.FromResult(_outcome).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId);
            var responseResult = result as JsonResult;

            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId,
            string actionPlanId, string outcomeId)
        {
            return await _function.RunAsync(
                _request,
                customerId,
                interactionId,
                actionPlanId,
                outcomeId).ConfigureAwait(false);
        }
    }
}