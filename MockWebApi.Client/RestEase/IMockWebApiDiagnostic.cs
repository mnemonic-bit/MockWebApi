using MockWebApi.Configuration.Model;
using RestEase;
using System.Threading.Tasks;

namespace MockWebApi.Client.RestEase
{
    [BasePath("diagnostic")]
    public interface IMockWebApiDiagnostic
    {

        [Get("diagnostic/client-infos")]
        Task<Response<string>> GetClientInfos();

        [Get("diagnostic/request-infos")]
        Task<Response<string>> GetRequestInfos();

        [Get("diagnostic/server-configuration")]
        Task<Response<string>> GetServerConfiguration();

        [Get("diagnostic/ping")]
        Task<Response<string>> Ping();

    }
}
