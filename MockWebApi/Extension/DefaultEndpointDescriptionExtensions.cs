using MockWebApi.Configuration.Model;

namespace MockWebApi.Extension
{
    internal static class DefaultEndpointDescriptionExtensions
    {

        public static void CopyTo(this DefaultEndpointDescription source, DefaultEndpointDescription destination)
        {
            BaseEndpointDescriptionExtensions.CopyTo(source, destination);
            destination.Result = source.Result;
        }

        public static void CopyTo(this DefaultEndpointDescription source, EndpointDescription destination)
        {
            BaseEndpointDescriptionExtensions.CopyTo(source, destination);
            destination.Results = new HttpResult[] { source.Result };
        }

    }
}
