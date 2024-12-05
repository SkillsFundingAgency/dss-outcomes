namespace NCS.DSS.Outcomes.Models
{
    public class OutcomesConfigurationSettings
    {
        public required string CollectionId { get; set; }
        public required string DatabaseId { get; set; }
        public required string ActionPlanCollectionId { get; set; }
        public required string ActionPlanDatabaseId { get; set; }
        public required string CustomerCollectionId { get; set; }
        public required string CustomerDatabaseId { get; set; }
        public required string InteractionCollectionId { get; set; }
        public required string InteractionDatabaseId { get; set; }
        public required string SessionCollectionId { get; set; }
        public required string SessionDatabaseId { get; set; }
        public required string QueueName { get; set; }
        public required string OutcomeConnectionString { get; set; }
        public required string ServiceBusConnectionString { get; set; }
    }
}