using System.ComponentModel;

namespace NCS.DSS.Outcomes.ReferenceData
{
    public enum OutcomeType
    {

        [Description("Customer Satisfaction Outcome")]
        CustomerSatisfactionOutcome = 1,

        [Description("Careers Management Outcome")]
        CareersManagementOutcome = 2,

        [Description("Job Outcome")]
        JobOutcome = 3,

        [Description("Learning Outcome")]
        LearningOutcome = 4,

    }
}
