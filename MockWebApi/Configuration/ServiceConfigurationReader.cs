﻿using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Routing;

namespace MockWebApi.Configuration
{
    public class ServiceConfigurationReader : IServiceConfigurationReader
    {

        private readonly IConfigurationCollection _serviceConfiguration;
        private readonly IRouteMatcher<EndpointDescription> _routeMatcher;

        public ServiceConfigurationReader(
            IConfigurationCollection serviceConfiguration,
            IRouteMatcher<EndpointDescription> routeMatcher)
        {
            _serviceConfiguration = serviceConfiguration;
            _routeMatcher = routeMatcher;
        }

        public void ConfigureService(ServiceConfiguration configuration)
        {
            _serviceConfiguration.Set(ConfigurationCollection.Parameters.TrackServiceApiCalls, configuration?.TrackServiceApiCalls ?? false);
            _serviceConfiguration.Set(ConfigurationCollection.Parameters.LogServiceApiCalls, configuration?.LogServiceApiCalls ?? false);
            _serviceConfiguration.Set(ConfigurationCollection.Parameters.DefaultHttpStatusCode, configuration?.DefaultHttpStatusCode ?? 200);
            _serviceConfiguration.Set(ConfigurationCollection.Parameters.DefaultContentType, configuration.DefaultContentType ?? "text/plain");

            if (configuration?.EndpointDescriptions == null)
            {
                return;
            }

            foreach (EndpointDescription endpoint in configuration.EndpointDescriptions)
            {
                _routeMatcher.AddRoute(endpoint.Route, endpoint);
            }
        }

    }
}