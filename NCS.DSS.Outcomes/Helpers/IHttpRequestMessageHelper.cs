﻿using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Outcomes.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetOutcomesFromRequest<T>(HttpRequestMessage req);
    }
}