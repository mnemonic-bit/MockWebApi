using System.Collections.Generic;

namespace MockWebApi.Configuration
{
    public class HostConfiguration : IHostConfiguration
    {

        private readonly IDictionary<string, IServiceConfiguration> _serviceConfigurations;

        public HostConfiguration()
        {
            _serviceConfigurations = new Dictionary<string, IServiceConfiguration>();
        }

        public void AddConfiguration(string serviceName, IServiceConfiguration serviceConfiguration)
        {
            _serviceConfigurations.Add(serviceName, serviceConfiguration);
        }

        public bool TryGetConfiguration(string serviceName, out IServiceConfiguration serviceConfiguration)
        {
            return _serviceConfigurations.TryGetValue(serviceName, out serviceConfiguration);
        }

    }
}
