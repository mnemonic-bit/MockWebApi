﻿using MockWebApi.Configuration.Model;
using System;

namespace MockWebApi.Configuration
{
    public class HostConfigurationReader : IHostConfigurationReader
    {

        private readonly IHostConfiguration _hostConfiguration;

        public HostConfigurationReader(
            IHostConfiguration hostConfiguration)
        {
            _hostConfiguration = hostConfiguration;
        }

        public void ConfigureHost(MockedHostConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), $"The host configuration must not be null.");
            }

            _hostConfiguration.TrackServiceApiCalls = configuration.TrackServiceApiCalls;
            _hostConfiguration.LogServiceApiCalls = configuration.LogServiceApiCalls;

            if (configuration.Services == null)
            {
                return;
            }

            foreach(var service in configuration.Services)
            {
                IServiceConfiguration serviceConfiguration = new ServiceConfiguration(service.ServiceName, service.BaseUrl);
                IServiceConfigurationReader serviceConfigurationReader = new ServiceConfigurationReader(serviceConfiguration);
                serviceConfigurationReader.ConfigureService(service);
                _hostConfiguration.AddConfiguration(service.ServiceName, serviceConfiguration);
            }
        }

    }
}
