using Azure.Identity;
using Azure.Messaging.ServiceBus;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.Cosmos.Provider;
using NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service;
using NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Models;
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
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    services.AddOptions<OutcomesConfigurationSettings>()
                        .Bind(configuration);

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
                        var logger = s.GetRequiredService<ILogger<Program>>();
                        var connectionString = configuration["OutcomeConnectionString"];
                        var endpoint = configuration["CosmosDbEndpoint"];

                        var options = new CosmosClientOptions
                        {
                            ConnectionMode = ConnectionMode.Gateway
                        };

                        if (!string.IsNullOrWhiteSpace(endpoint))
                        {
                            logger.LogInformation("Using DefaultAzureCredential for Cosmos DB (managed identity)");
                            return new CosmosClient(endpoint, new DefaultAzureCredential(), options);
                        }
                        else if (!string.IsNullOrWhiteSpace(connectionString))
                        {
                            logger.LogInformation("No managed identity found: using Cosmos DB connection string (local development)");
                            return new CosmosClient(connectionString, options);
                        }
                        else
                        {
                            throw new InvalidOperationException("Neither CosmosDbEndpoint or a ConnectionString are configured");
                        }
                    });

                    services.AddSingleton(s =>
                    {
                        var settings = s.GetRequiredService<IOptions<OutcomesConfigurationSettings>>().Value;
                        return new ServiceBusClient(settings.ServiceBusConnectionString);
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
