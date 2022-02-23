using System;

using MockWebApi.Configuration.Model;

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

        public void ConfigureService(MockedServiceConfiguration configuration, bool overwriteExisting = true)
        {
            if (overwriteExisting)
            {
                //_serviceConfiguration.ServiceName = configuration.ServiceName;
                //_serviceConfiguration.Url = configuration.BaseUrl;
                _serviceConfiguration.DefaultEndpointDescription = configuration.DefaultEndpointDescription;
            }
            else
            {
                //_serviceConfiguration.ServiceName ??= configuration.ServiceName;
                //_serviceConfiguration.Url ??= configuration.BaseUrl;
                _serviceConfiguration.DefaultEndpointDescription ??= configuration.DefaultEndpointDescription;
            }

            if (configuration?.EndpointDescriptions == null)
            {
                return;
            }

            foreach (EndpointDescription endpoint in configuration.EndpointDescriptions)
            {
                _serviceConfiguration.RouteMatcher.AddRoute(endpoint.Route, new EndpointState(endpoint));
            }
        }

    }
}
