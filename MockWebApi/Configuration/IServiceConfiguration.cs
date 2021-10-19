using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Routing;

namespace MockWebApi.Configuration
{
    public interface IServiceConfiguration
    {

        DefaultEndpointDescription DefaultEndpointDescription { get; set; }

        IConfigurationCollection ConfigurationCollection { get; }

        IRouteMatcher<EndpointDescription> RouteMatcher { get; }

        void ResetToDefault();

        bool ReadFromYaml(string configYaml);

    }
}
