using MockWebApi.Configuration.Model;

namespace MockWebApi.Extension
{
    internal static class EndpointDescriptionExtensions
    {

        public static void CopyTo(this EndpointDescription source, EndpointDescription destination)
        {
            BaseEndpointDescriptionExtensions.CopyTo(source, destination);

            destination.Route = source.Route;
            destination.HttpMethod = source.HttpMethod;
            destination.Parameters = source.Parameters;
            destination.RequestBodyType = source.RequestBodyType;
            destination.LifecyclePolicy = source.LifecyclePolicy;
            destination.Results = source.Results;
            destination.PersistRequestInformation = source.PersistRequestInformation;
            destination.LogRequestInformation = source.LogRequestInformation;
        }

    }
}
