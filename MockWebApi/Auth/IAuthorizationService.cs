using MockWebApi.Configuration.Model;

namespace MockWebApi.Auth
{
    public interface IAuthorizationService
    {

        bool CkeckAuthorization(string authorizationHeader, EndpointDescription endpointDescription);

    }
}
