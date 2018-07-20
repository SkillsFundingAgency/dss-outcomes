using System;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Outcomes.Cosmos.Helper;
using NCS.DSS.Outcomes.GetOutcomesByIdHttpTrigger.Service;
using NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Helpers;
using NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service;
using NCS.DSS.Outcomes.Validation;


namespace NCS.DSS.Outcomes.Ioc
{
    public class RegisterServiceProvider
    {
        public IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient<IGetOutcomesHttpTriggerService, GetOutcomesHttpTriggerService>();
            services.AddTransient<IGetOutcomesByIdHttpTriggerService, GetOutcomesByIdHttpTriggerService>();
            services.AddTransient<IPostOutcomesHttpTriggerService, PostOutcomesHttpTriggerService>();
            services.AddTransient<IPatchOutcomesHttpTriggerService, PatchOutcomesHttpTriggerService>();
            services.AddTransient<IResourceHelper, ResourceHelper>();
            services.AddTransient<IValidate, Validate>();
            services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();
            return services.BuildServiceProvider(true);
        }
    }
}
