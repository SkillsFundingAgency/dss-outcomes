using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.PostOutcomesHttpTrigger.Service
{
    public interface IPostOutcomesHttpTriggerService
    {
        Task<Models.Outcomes> CreateAsync(Models.Outcomes Outcomes);
    }
}