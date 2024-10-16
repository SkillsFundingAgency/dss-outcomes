namespace NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Service
{
    public interface IGetOutcomesHttpTriggerService
    {
        Task<List<Models.Outcomes>> GetOutcomesAsync(Guid customerId);
    }
}