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

    }
}
