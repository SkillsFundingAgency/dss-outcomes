using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.Outcomes.ServiceBus
{
    public static class ServiceBusClient
    {
        public static async Task SendPostMessageAsync(Models.Outcomes outcomes, string reqUrl, IQueueClient queueClient)
        {
            
            var messageModel = new MessageModel()
            {
                TitleMessage = "New Outcome record {" + outcomes.OutcomeId + "} added at " + DateTime.UtcNow,
                CustomerGuid = outcomes.CustomerId,
                LastModifiedDate = outcomes.LastModifiedDate,
                URL = reqUrl + "/" + outcomes.OutcomeId,
                IsNewCustomer = false,
                TouchpointId = outcomes.LastModifiedTouchpointId
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = outcomes.CustomerId + " " + DateTime.UtcNow
            };

            await queueClient.SendAsync(msg);
        }

        public static async Task SendPatchMessageAsync(Models.Outcomes outcomes, Guid customerId, string reqUrl, IQueueClient queueClient)
        {

            var messageModel = new MessageModel
            {
                TitleMessage = "Outcome record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = outcomes.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = outcomes.LastModifiedTouchpointId
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            await queueClient.SendAsync(msg);
        }
    }

    public class MessageModel
    {
        public string TitleMessage { get; set; }
        public Guid? CustomerGuid { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string URL { get; set; }
        public bool IsNewCustomer { get; set; }
        public string TouchpointId { get; set; }
    }
}