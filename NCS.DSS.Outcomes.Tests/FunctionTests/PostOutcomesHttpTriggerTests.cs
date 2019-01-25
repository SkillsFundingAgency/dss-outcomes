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
using NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.Outcomes.Tests.FunctionTests
{
    [TestFixture]
    public class PostOutcomesHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidActionPlanId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string ValidSessionId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string InValidId = "b3ced659-fad9-4234-b3cc-d4d40f687442";
        private ILogger _log;
        private HttpRequest _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;

        private IPostOutcomesHttpTriggerService _postOutcomesHttpTriggerService;
        private Models.Outcomes _outcome;

        [SetUp]
        public void Setup()
        {
            _outcome = Substitute.For<Models.Outcomes>();

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
            _postOutcomesHttpTriggerService = Substitute.For<IPostOutcomesHttpTriggerService>();
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssApimUrl(_request).Returns("http://localhost:7071/");
        }

        [Test]
        public async Task PostOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            _httpResponseMessageHelper
                .BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionPlanId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenActionPlanIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }
        
        [Test]
        public async Task PostOutcomesHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenOutcomesHasFailedValidation()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.Outcomes>(_request).Returns(Task.FromResult(_outcome).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.Outcomes>()).Returns(validationResults);

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<List<ValidationResult>>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostOutcomesHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenOutcomesRequestIsInvalid()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.Outcomes>(_request).Throws(new JsonException());

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<JsonException>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostOutcomesHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.Outcomes>(_request).Returns(Task.FromResult(_outcome).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostOutcomesHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.Outcomes>(_request).Returns(Task.FromResult(_outcome).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetOutcomesByIdHttpTrigger_ReturnsStatusCodeOk_WhenActionPlanDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.Outcomes>(_request).Returns(Task.FromResult(_outcome).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostOutcomesHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateOutcomesRecord()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.Outcomes>(_request).Returns(Task.FromResult(_outcome).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _postOutcomesHttpTriggerService.CreateAsync(Arg.Any<Models.Outcomes>()).Returns(Task.FromResult<Models.Outcomes>(null).Result);

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostOutcomesHttpTrigger_ReturnsBadRequestStatusCode_WhenRequestIsInValid()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.Outcomes>(_request).Returns(Task.FromResult(_outcome).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _postOutcomesHttpTriggerService.CreateAsync(Arg.Any<Models.Outcomes>()).Returns(Task.FromResult<Models.Outcomes>(null).Result);

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostOutcomesHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.Outcomes>(_request).Returns(Task.FromResult(_outcome).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
             _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _postOutcomesHttpTriggerService.CreateAsync(Arg.Any<Models.Outcomes>()).Returns(Task.FromResult<Models.Outcomes>(_outcome).Result);

            _httpResponseMessageHelper
                .Created(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.Created));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string actionPlanId, string sessionId)
        {
            return await PostOutcomesHttpTrigger.Function.PostOutcomesHttpTrigger.Run(
                _request,
                _log,
                customerId,
                interactionId,
                actionPlanId,
                sessionId,
                _resourceHelper,
                _postOutcomesHttpTriggerService,
                _validate,
                _loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                _jsonHelper).ConfigureAwait(false);
        }

    }
}