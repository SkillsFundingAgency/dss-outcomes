using System;

namespace NCS.DSS.Outcome.PostOutcomeHttpTrigger
{
    public class PostOutcomeHttpTriggerService
    {
        public Guid? Create(Models.Outcome outcome)
        {
            if (outcome == null)
                return null;

            return Guid.NewGuid();
        }
    }
}