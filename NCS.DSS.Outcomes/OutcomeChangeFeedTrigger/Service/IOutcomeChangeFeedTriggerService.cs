using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace NCS.DSS.Outcomes.OutcomeChangeFeedTrigger.Service
{
    public interface IOutcomeChangeFeedTriggerService
    {
        Task SendMessageToChangeFeedQueueAsync(Document document);
    }
}
