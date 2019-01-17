using System.Net;
using System.Net.Http;
using System.Reflection;
using DFC.Functions.DI.Standard.Attributes;
using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace NCS.DSS.Outcomes.APIDefinition
{
    public static class GenerateOutcomeSwaggerDoc
    {
        public const string APITitle = "Outcomes";
        public const string APIDefinitionName = "API-Definition";
        public const string APIDefRoute = APITitle + "/" + APIDefinitionName;
        public const string APIDescription = "Basic details of a National Careers Service " + APITitle + " Resource";
      
        [FunctionName(APIDefinitionName)]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = APIDefRoute)]HttpRequest req,
            [Inject]ISwaggerDocumentGenerator swaggerDocumentGenerator)
        {
           var swagger = swaggerDocumentGenerator.GenerateSwaggerDocument(req, APITitle, APIDescription, APIDefinitionName, Assembly.GetExecutingAssembly());

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(swagger)
            };
        }
    }
}