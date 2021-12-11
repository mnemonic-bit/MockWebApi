using MockWebApi.Configuration.Model;
using MockWebApi.Routing;

namespace MockWebApi.Tests.TestUtils
{
    public static class RouteGraphMatcherExtensions
    {

        public static void AddRoute(this RouteGraphMatcher<EndpointDescription> graphMatcher, string path)
        {
            EndpointDescription endpointDescription = new EndpointDescription()
            {
                Route = path
            };

            graphMatcher.AddRoute(path, endpointDescription);
        }

    }
}
