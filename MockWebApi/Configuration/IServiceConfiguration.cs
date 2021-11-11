using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Routing;

namespace MockWebApi.Configuration
{
    public interface IServiceConfiguration
    {

        string ServiceName { get; set; }

        string Url { get; set; }

        DefaultEndpointDescription DefaultEndpointDescription { get; set; }

        //TODO: create a class of its own for the error response
        DefaultEndpointDescription ErrorResponseEndpointDescription { get; set; }

        JwtServiceOptions JwtServiceOptions { get; set; }

        IConfigurationCollection ConfigurationCollection { get; }

        IRouteMatcher<EndpointDescription> RouteMatcher { get; }

        void ResetToDefault();

        bool ReadFromYaml(string configYaml);

    }
}
