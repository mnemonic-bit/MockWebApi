using MockWebApi.Configuration.Model;
using MockWebApi.Data;

namespace MockWebApi.Configuration
{
    public class ServiceConfigurationReader : IServiceConfigurationReader
    {

        private readonly IServiceConfiguration _serviceConfiguration;

        public ServiceConfigurationReader(
            IServiceConfiguration serviceConfiguration)
        {
            _serviceConfiguration = serviceConfiguration;
        }

        public void ConfigureService(MockedWebApiServiceConfiguration configuration)
        {
            _serviceConfiguration.ConfigurationCollection.Set(ConfigurationCollection.Parameters.TrackServiceApiCalls, configuration?.TrackServiceApiCalls ?? false);
            _serviceConfiguration.ConfigurationCollection.Set(ConfigurationCollection.Parameters.LogServiceApiCalls, configuration?.LogServiceApiCalls ?? false);
            _serviceConfiguration.ConfigurationCollection.Set(ConfigurationCollection.Parameters.DefaultHttpStatusCode, configuration?.DefaultEndpointDescription?.Result?.StatusCode ?? System.Net.HttpStatusCode.OK);
            _serviceConfiguration.ConfigurationCollection.Set(ConfigurationCollection.Parameters.DefaultContentType, configuration?.DefaultEndpointDescription?.Result?.ContentType ?? "text/plain");

            if (configuration?.EndpointDescriptions == null)
            {
                return;
            }

            foreach (EndpointDescription endpoint in configuration.EndpointDescriptions)
            {
                _serviceConfiguration.RouteMatcher.AddRoute(endpoint.Route, endpoint);
            }
        }

    }
}
