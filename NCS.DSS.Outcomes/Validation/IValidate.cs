using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Outcomes.Models;

namespace NCS.DSS.Outcomes.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IOutcomes resource, DateTime? dateAndTimeSessionCreated);
    }
}