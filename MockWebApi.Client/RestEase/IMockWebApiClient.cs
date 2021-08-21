using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockWebApi.Client.RestEase
{
    [BasePath("service-api")]
    public interface IMockWebApiClient
    {

        [Post("configure")]
        Task<Response<string>> Configure([Query] int? DefaultHttpStatusCode = null, [Query] string DefaultContentType = null, [Query] bool? TrackServiceApiCalls = null, [Query] bool? LogServiceApiCalls = null);

        [Get("configure")]
        Task<Response<string>> GetConfiguration();

        [Get("configure/route")]
        Task<Response<string>> GetRoutes();

        [Post("configure/route")]
        Task<Response<string>> ConfigureRoute([Body] string endpointConfiguration);

        [Get("request")]
        Task<Response<string>> GetAllRequests();

        [Get("request/tail/{count?}")]
        Task<Response<string>> GetLastRequests([Path("count?")] int? count);

    }
}
