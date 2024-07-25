﻿using System.Net;
using System.Net.Http;
using System.Reflection;
using DFC.Functions.DI.Standard.Attributes;
using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Mvc;

namespace NCS.DSS.Outcomes.APIDefinition
{
    public class GenerateOutcomeSwaggerDoc
    {
        private readonly ISwaggerDocumentGenerator _swaggerDocumentGenerator;
        public GenerateOutcomeSwaggerDoc(ISwaggerDocumentGenerator swaggerDocumentGenerator) 
        {
            _swaggerDocumentGenerator = swaggerDocumentGenerator; 
        }
        public const string ApiTitle = "Outcomes";
        public const string ApiDefinitionName = "API-Definition";
        public const string ApiDefRoute = ApiTitle + "/" + ApiDefinitionName;
        public const string ApiDescription = "To support the Data Collections integration with DSS ClaimedPriorityGroup has been removed and IsPriorityCustomer " +
            "has been added as a true/false value.";
        public const string ApiVersion = "3.0.0";

        [Function(ApiDefinitionName)]
        public IActionResult RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiDefRoute)]HttpRequest req)
        {
            var swagger = _swaggerDocumentGenerator.GenerateSwaggerDocument(req, ApiTitle, ApiDescription,
                ApiDefinitionName, ApiVersion, Assembly.GetExecutingAssembly());

            if (string.IsNullOrEmpty(swagger))
                return new NoContentResult();

            return new OkObjectResult(swagger);
        }
    }
}