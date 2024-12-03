using Azure.Messaging.ServiceBus;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service;
using NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.ServiceBus;
using NCS.DSS.Outcomes.Validation;

namespace NCS.DSS.Outcomes
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddSingleton<IResourceHelper, ResourceHelper>();
                    services.AddSingleton<IValidate, Validate>();
                    services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
                    services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
                    services.AddSingleton<IJsonHelper, JsonHelper>();
                    services.AddSingleton<IDynamicHelper, DynamicHelper>();
                    services.AddSingleton<ICosmosDBProvider, CosmosDBProvider>();
                    services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
                    services.AddScoped<IGetOutcomesHttpTriggerService, GetOutcomesHttpTriggerService>();
                    services.AddScoped<IGetOutcomesByIdHttpTriggerService, GetOutcomesByIdHttpTriggerService>();
                    services.AddScoped<IPostOutcomesHttpTriggerService, PostOutcomesHttpTriggerService>();
                    services.AddScoped<IPatchOutcomesHttpTriggerService, PatchOutcomesHttpTriggerService>();
                    services.AddScoped<IOutcomePatchService, OutcomePatchService>();
                    services.AddScoped<IOutcomesServiceBusClient, OutcomesServiceBusClient>();

                    services.AddSingleton(s =>
                    {
                        var options = new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway };
                        var outcomeConnectionString = Environment.GetEnvironmentVariable("OutcomeConnectionString");

                        return new CosmosClient(outcomeConnectionString, options);
                    });

                    services.AddSingleton(s =>
                    {
                        var serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
                        return new ServiceBusClient(serviceBusConnectionString);
                    });

                    services.Configure<LoggerFilterOptions>(options =>
                    {
                        LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                        if (toRemove is not null)
                        {
                            options.Rules.Remove(toRemove);
                        }
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}
