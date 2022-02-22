using System.Collections.Generic;

namespace MockWebApi.Configuration
{
    public interface IHostConfiguration
    {

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

        IEnumerable<IServiceConfiguration> Configurations { get; }

        void AddConfiguration(string serviceName, IServiceConfiguration serviceConfiguration);

        bool RemoveConfiguration(string serviceName);

        bool TryGetConfiguration(string serviceName, out IServiceConfiguration? serviceConfiguration);

    }
}