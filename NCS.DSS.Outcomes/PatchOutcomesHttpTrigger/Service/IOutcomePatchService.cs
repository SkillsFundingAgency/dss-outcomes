using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCS.DSS.Outcomes.Models;

namespace NCS.DSS.Outcomes.PatchOutcomesHttpTrigger.Service
{
    public interface IOutcomePatchService
    {
        string Patch(string outcomeJson, OutcomesPatch outcomePatch);
    }
}
