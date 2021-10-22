using MockWebApi.Configuration.Model;
using RestEase;
using System.Threading.Tasks;

namespace MockWebApi.Client.RestEase
{
    [BasePath("service-api")]
    public interface IMockWebApiClient
    {

        [Get("configure")]
        Task<Response<string>> DownloadConfiguration();

        [Post("configure")]
        Task<Response<string>> UploadConfiguration([Body] string configAsYanml);

        [Delete("configure")]
        Task<Response<string>> ResetConfiguration([Body] string configAsYanml);

        [Get("configure/route")]
        Task<Response<string>> GetRoutes();

        [Post("configure/route")]
        Task<Response<string>> ConfigureRoute([Body] string endpointConfiguration);

        [Delete("configure/route")]
        Task<Response<string>> DeleteRoute([Body] string routeKey);

        [Get("request")]
        Task<Response<string>> GetAllRequests();

        [Get("request/tail/{count?}")]
        Task<Response<string>> GetLastRequests([Path("count?")] int? count);

        [Post("configure/jwt")]
        public Task<Response<string>> GetJwtToken([Body] JwtCredentialUser user);

    }
}
