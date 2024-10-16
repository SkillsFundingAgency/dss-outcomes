using NCS.DSS.Outcomes.Models;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Outcomes.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IOutcomes resource, DateTime? dateAndTimeSessionCreated);
    }
}