namespace NCS.DSS.Outcomes.ServiceBus
{
    public interface IOutcomesServiceBusClient
    {
        Task SendPatchMessageAsync(Models.Outcomes outcomes, Guid customerId, string reqUrl);
        Task SendPostMessageAsync(Models.Outcomes outcomes, string reqUrl);
    }
}