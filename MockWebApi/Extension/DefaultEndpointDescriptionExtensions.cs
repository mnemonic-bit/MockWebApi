using MockWebApi.Configuration.Model;

namespace MockWebApi.Extension
{
    internal static class DefaultEndpointDescriptionExtensions
    {

        public static void CopyTo(this DefaultEndpointDescription source, DefaultEndpointDescription destination)
        {
            destination.AllowedUsers = source.AllowedUsers;
            destination.CheckAuthorization = source.CheckAuthorization;
            destination.Result = source.Result;
            destination.ReturnCookies = source.ReturnCookies;
        }

    }
}
