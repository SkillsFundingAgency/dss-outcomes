using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using NCS.DSS.Outcomes.Ioc;
using System.Net;
using System.Net.Http;
using System.Reflection;
using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using DFC.Functions.DI.Standard.Attributes;

namespace NCS.DSS.Outcomes.APIDefinition
{
    public static class ApiDefinition
    {
        public const string APITitle = "Outcomes";
        public const string APIDefinitionName = "API-Definition";
        public const string APIDefRoute = APITitle + "/" + APIDefinitionName;
        public const string APIDescription = "Basic details of a National Careers Service " + APITitle + " Resource";
        public const string APIVersuin = "2.0.0";
        [FunctionName(APIDefinitionName)]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = APIDefRoute)] HttpRequest req,
    [Inject] ISwaggerDocumentGenerator swaggerDocumentGenerator)
        {
            var swagger = swaggerDocumentGenerator.GenerateSwaggerDocument(req, APITitle, APIDescription,
                APIDefinitionName, APIVersuin, Assembly.GetExecutingAssembly());

            if (string.IsNullOrEmpty(swagger))
                return new HttpResponseMessage(HttpStatusCode.NoContent);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(swagger)
            };
        }
    }
}