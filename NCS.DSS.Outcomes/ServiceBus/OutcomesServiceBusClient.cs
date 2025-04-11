using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Outcomes.Models;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.Outcomes.ServiceBus
{
    public class OutcomesServiceBusClient : IOutcomesServiceBusClient
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<OutcomesServiceBusClient> _logger;
        private readonly string _queueName;

        public OutcomesServiceBusClient(ServiceBusClient serviceBusClient,
            IOptions<OutcomesConfigurationSettings> configOptions,
            ILogger<OutcomesServiceBusClient> logger)
        {
            var config = configOptions.Value;
            if (string.IsNullOrEmpty(config.QueueName))
            {
                throw new ArgumentNullException(nameof(config.QueueName), "QueueName cannot be null or empty.");
            }

            _serviceBusClient = serviceBusClient;
            _queueName = config.QueueName;
            _logger = logger;
        }

        public async Task SendPostMessageAsync(Models.Outcomes outcomes, string reqUrl)
        {
            var serviceBusSender = _serviceBusClient.CreateSender(_queueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New Outcome record {" + outcomes.OutcomeId + "} added at " + DateTime.UtcNow,
                CustomerGuid = outcomes.CustomerId,
                LastModifiedDate = outcomes.LastModifiedDate,
                URL = reqUrl + "/" + outcomes.OutcomeId,
                IsNewCustomer = false,
                TouchpointId = outcomes.LastModifiedTouchpointId
            };

            var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = outcomes.CustomerId + " " + DateTime.UtcNow
            };

            _logger.LogInformation("Attempting to send POST message to service bus. Outcome ID: {OutcomeId}", outcomes.OutcomeId);

            await serviceBusSender.SendMessageAsync(msg);

            _logger.LogInformation("Successfully sent POST message to the service bus. Outcome ID: {OutcomeId}", outcomes.OutcomeId);
        }

        public async Task SendPatchMessageAsync(Models.Outcomes outcomes, Guid customerId, string reqUrl)
        {
            var serviceBusSender = _serviceBusClient.CreateSender(_queueName);

            var messageModel = new MessageModel
            {
                TitleMessage = "Outcome record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = outcomes.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = outcomes.LastModifiedTouchpointId
            };

            var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            _logger.LogInformation("Attempting to send PATCH message to service bus. Outcome ID: {OutcomeId}.", outcomes.OutcomeId);

            await serviceBusSender.SendMessageAsync(msg);

            _logger.LogInformation("Successfully sent PATCH message to the service bus. Outcome ID: {OutcomeId}", outcomes.OutcomeId);
        }
    }
}