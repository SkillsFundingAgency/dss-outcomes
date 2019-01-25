using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.Models;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

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
        private ILogger _log;
        private HttpRequest _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private IPatchOutcomesHttpTriggerService _patchOutcomesHttpTriggerService;
        private Models.Outcomes _outcome;
        private OutcomesPatch _outcomePatch;

        [SetUp]
        public void Setup()
        {
            _outcome = Substitute.For<Models.Outcomes>();
            _outcomePatch = Substitute.For<OutcomesPatch>();

            _request = new DefaultHttpRequest(new DefaultHttpContext());

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _validate = Substitute.For<IValidate>();
            _loggerHelper = Substitute.For<ILoggerHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();
            _jsonHelper = Substitute.For<IJsonHelper>();
            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _patchOutcomesHttpTriggerService = Substitute.For<IPatchOutcomesHttpTriggerService>();
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssApimUrl(_request).Returns("http://localhost:7071/");
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            _httpResponseMessageHelper
                .BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }
        
        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionPlanId, ValidOutcomeId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenAcionPlanIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId, ValidOutcomeId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenOutcomeIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, InValidId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenOutcomesHasFailedValidation()
        {
            _httpRequestHelper.GetResourceFromRequest<OutcomesPatch>(_request).Returns(Task.FromResult(_outcomePatch).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.ValidateResource(Arg.Any<OutcomesPatch>()).Returns(validationResults);

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<List<ValidationResult>>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenOutcomesRequestIsInvalid()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.Outcomes>(_request).Throws(new JsonException());

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<HttpRequest>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<OutcomesPatch>(_request).Returns(Task.FromResult(_outcomePatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }


        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<OutcomesPatch>(_request).Returns(Task.FromResult(_outcomePatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
             _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeNoContent_WhenActionPlanDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<OutcomesPatch>(_request).Returns(Task.FromResult(_outcomePatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
             _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

            _patchOutcomesHttpTriggerService.GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Outcomes>(null).Result);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }


        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeOk_WhenOutcomesDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<OutcomesPatch>(_request).Returns(Task.FromResult(_outcomePatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
             _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _patchOutcomesHttpTriggerService.GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Outcomes>(null).Result);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateOutcomesRecord()
        {
            _httpRequestHelper.GetResourceFromRequest<OutcomesPatch>(_request).Returns(Task.FromResult(_outcomePatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
             _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _patchOutcomesHttpTriggerService.GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(),Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_outcome).Result);

            _patchOutcomesHttpTriggerService.UpdateAsync(Arg.Any<Models.Outcomes>(), Arg.Any<OutcomesPatch>()).Returns(Task.FromResult<Models.Outcomes>(null).Result);

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenRequestIsNotValid()
        {
            _httpRequestHelper.GetResourceFromRequest<OutcomesPatch>(_request).Returns(Task.FromResult(_outcomePatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
             _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _patchOutcomesHttpTriggerService.GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_outcome).Result);

            _patchOutcomesHttpTriggerService.UpdateAsync(Arg.Any<Models.Outcomes>(), Arg.Any<OutcomesPatch>()).Returns(Task.FromResult<Models.Outcomes>(null).Result);

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchOutcomesHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            _httpRequestHelper.GetResourceFromRequest<OutcomesPatch>(_request).Returns(Task.FromResult(_outcomePatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
             _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _patchOutcomesHttpTriggerService.GetOutcomesForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_outcome).Result);

            _patchOutcomesHttpTriggerService.UpdateAsync(Arg.Any<Models.Outcomes>(), Arg.Any<OutcomesPatch>()).Returns(Task.FromResult(_outcome).Result);

            _httpResponseMessageHelper
                .Ok(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.OK));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidOutcomeId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string actionPlanId, string outcomeId, string sessionId)
        {
            return await PatchOutcomesHttpTrigger.Function.PatchOutcomesHttpTrigger.Run(
                _request,
                _log,
                customerId,
                interactionId,
                actionPlanId,
                outcomeId,
                sessionId,
                _resourceHelper,
                _patchOutcomesHttpTriggerService,
                _validate,
                _loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                _jsonHelper).ConfigureAwait(false);
        }

    }
}