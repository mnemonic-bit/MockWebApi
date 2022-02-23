using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Routing;

namespace MockWebApi.Configuration
{
    public interface IServiceConfiguration
    {

        string ServiceName { get; }

        string Url { get; }

        DefaultEndpointDescription DefaultEndpointDescription { get; set; }

        //TODO: create a class of its own for the error response
        DefaultEndpointDescription ErrorResponseEndpointDescription { get; set; }

        JwtServiceOptions JwtServiceOptions { get; set; }

        IConfigurationCollection ConfigurationCollection { get; }

        IRouteMatcher<IEndpointState> RouteMatcher { get; }

        void ResetToDefault();

        bool ReadFromYaml(string configYaml);

    }
}
