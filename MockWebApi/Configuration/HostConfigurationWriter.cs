﻿using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using System.Linq;

namespace MockWebApi.Configuration
{
    public class HostConfigurationWriter : IHostConfigurationWriter
    {

        private readonly IConfigurationWriter _configurationWriter;
        private readonly IHostConfiguration _hostConfiguration;

        public HostConfigurationWriter(
            IConfigurationWriter configurationWriter,
            IHostConfiguration hostConfiguration)
        {
            _configurationWriter = configurationWriter;
            _hostConfiguration = hostConfiguration;
        }

        public string WriteConfiguration(MockedHostConfiguration serviceConfiguration, string outputFormat = "YAML")
        {
            return _configurationWriter.WriteConfiguration(serviceConfiguration, outputFormat.ToUpper());
        }

        public MockedHostConfiguration GetHostConfiguration()
        {
            MockedHostConfiguration serviceConfiguration = new MockedHostConfiguration
            {
                TrackServiceApiCalls = _hostConfiguration.TrackServiceApiCalls,
                LogServiceApiCalls = _hostConfiguration.LogServiceApiCalls,

                Services = null
            };

            return serviceConfiguration;
        }

    }
}
