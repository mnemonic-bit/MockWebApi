using System.Collections.Generic;

namespace MockWebApi.Configuration
{
    /// <summary>
    /// Stores the configuration of a complete mock-host.
    /// </summary>
    public class HostConfiguration : IHostConfiguration
    {

        internal const string DEFAULT_HOST_IP_AND_PORT = "http://0.0.0.0:6000;https://0.0.0.0:6001";

        public HostConfiguration()
        {
            _serviceConfigurations = new Dictionary<string, IServiceConfiguration>();
        }

        /// <summary>
        /// A flag which is used to set the server into tracking mode, i.e. all
        /// calls to the service API will also be written to the request-history.
        /// </summary>
        public bool? TrackServiceApiCalls { get; set; }

        /// <summary>
        /// A flag which lets the service log each request to the service API to
        /// the console, similar to what the mocked API does for mocked endpoints.
        /// </summary>
        public bool? LogServiceApiCalls { get; set; }

        public IEnumerable<IServiceConfiguration> Configurations => _serviceConfigurations.Values;

        public void AddConfiguration(string serviceName, IServiceConfiguration serviceConfiguration)
        {
            _serviceConfigurations.Add(serviceName, serviceConfiguration);
        }

        public bool RemoveConfiguration(string serviceName)
        {
            return _serviceConfigurations.Remove(serviceName);
        }

        public bool TryGetConfiguration(string serviceName, out IServiceConfiguration? serviceConfiguration)
        {
            return _serviceConfigurations.TryGetValue(serviceName, out serviceConfiguration);
        }


        private readonly IDictionary<string, IServiceConfiguration> _serviceConfigurations;


    }
}
