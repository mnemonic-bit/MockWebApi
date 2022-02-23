using MockWebApi.Configuration.Model;

namespace MockWebApi.Extension
{
    internal static class BaseEndpointDescriptionExtensions
    {

        public static void CopyTo(this BaseEndpointDescription source, BaseEndpointDescription destination)
        {
            destination.AllowedUsers = source.AllowedUsers;
            destination.CheckAuthorization = source.CheckAuthorization;
            destination.ReturnCookies = source.ReturnCookies;
        }

    }
}
