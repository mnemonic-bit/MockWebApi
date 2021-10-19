using MockWebApi.Configuration.Model;
using MockWebApi.Data;

namespace MockWebApi.Configuration
{
    public interface IServiceConfiguration
    {

        DefaultEndpointDescription DefaultEndpointDescription { get; set; }

        IConfigurationCollection ConfigurationCollection { get; }

    }
}
