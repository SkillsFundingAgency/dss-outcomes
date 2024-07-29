using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service;
using NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Validation;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddLogging();
        services.AddSingleton<IResourceHelper, ResourceHelper>();
        services.AddSingleton<IValidate, Validate>();
        services.AddSingleton<ILoggerHelper, LoggerHelper>();
        services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
        services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
        services.AddSingleton<IJsonHelper, JsonHelper>();
        services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();
        services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
        services.AddScoped<IGetOutcomesHttpTriggerService, GetOutcomesHttpTriggerService>();
        services.AddScoped<IGetOutcomesByIdHttpTriggerService, GetOutcomesByIdHttpTriggerService>();
        services.AddScoped<IPostOutcomesHttpTriggerService, PostOutcomesHttpTriggerService>();
        services.AddScoped<IPatchOutcomesHttpTriggerService, PatchOutcomesHttpTriggerService>();
        services.AddScoped<IPatchOutcomesHttpTriggerService, PatchOutcomesHttpTriggerService>();
        services.AddScoped<IOutcomePatchService, OutcomePatchService>();
    })
    .Build();

host.Run();
