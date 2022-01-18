using MockWebApi.Configuration.Model;
using RestEase;
using System.Threading.Tasks;

namespace MockWebApi.Client.RestEase
{
    [BasePath("api")]
    public interface IMockWebApiClient
    {

        [Post("{serviceName}/start")]
        Task<Response<string>> StartNewMockApi([Path] string serviceName, [Query] string serviceUrl, [Body] string configuration = null);

        [Post("{serviceName}/stop")]
        Task<Response<string>> StopMockApi([Path] string serviceName);

        [Get("{serviceName}/configure")]
        Task<Response<string>> DownloadConfiguration([Path] string serviceName);

        [Post("{serviceName}/configure")]
        Task<Response<string>> UploadConfiguration([Path] string serviceName, [Body] string configAsYanml);

        [Delete("{serviceName}/configure")]
        Task<Response<string>> ResetConfiguration([Path] string serviceName, [Body] string configAsYanml);

        [Post("{serviceName}/configure/jwt")]
        public Task<Response<string>> GetJwtToken([Path] string serviceName, [Body] JwtCredentialUser user);

        [Get("{serviceName}/configure/route")]
        Task<Response<string>> GetRoutes([Path] string serviceName);

        [Post("{serviceName}/configure/route")]
        Task<Response<string>> ConfigureRoute([Path] string serviceName, [Body] string endpointConfiguration);

        [Delete("{serviceName}/configure/route")]
        Task<Response<string>> DeleteRoute([Path] string serviceName, [Body] string routeKey);

        [Get("{serviceName}/request")]
        Task<Response<string>> GetAllRequests([Path] string serviceName);

        [Get("{serviceName}/request/tail/{count?}")]
        Task<Response<string>> GetLastRequests([Path] string serviceName, [Path("count?")] int? count);

    }
}
