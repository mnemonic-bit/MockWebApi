using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Routing;

namespace MockWebApi.Configuration
{
    public interface IRestServiceConfiguration : IServiceConfiguration
    {

        DefaultEndpointDescription DefaultEndpointDescription { get; set; }

        //TODO: create a class of its own for the error response
        DefaultEndpointDescription ErrorResponseEndpointDescription { get; set; }

        JwtServiceOptions JwtServiceOptions { get; set; }

        IConfigurationCollection ConfigurationCollection { get; }

        IRouteMatcher<IEndpointState> RouteMatcher { get; }

    }
}
