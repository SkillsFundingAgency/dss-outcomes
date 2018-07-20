using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.Helpers
{
    public class HttpRequestMessageHelper : IHttpRequestMessageHelper
    {
        public async Task<T> GetOutcomesFromRequest<T>(HttpRequestMessage req)
        {
            return await req.Content.ReadAsAsync<T>();
        }
    }
}
