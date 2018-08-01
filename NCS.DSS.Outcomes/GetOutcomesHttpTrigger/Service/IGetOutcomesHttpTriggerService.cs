using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.GetOutcomesHttpTrigger.Service
{
    public interface IGetOutcomesHttpTriggerService
    {
        Task<List<Models.Outcomes>> GetOutcomesAsync(Guid customerId);
    }
}