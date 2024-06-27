using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace MockWebApi.Configuration.Model
{
    /// <summary>
    /// Defines the configuration of mocked services in general. This class is
    /// the base-class of all configuration classes for all kinds of mocked
    /// services.
    /// </summary>
    public abstract class MockedServiceConfiguration
    {

        /// <summary>
        /// The name of the service to uniquely identify a service.
        /// If no name was given, the server will choose a GUID as the
        /// name.
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// The URL this service will bind to, which may include
        /// the scheme, IP address, and port number. Any local paths will
        /// not be considered in this URL.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// The ServiceType defines the kind of service to start up.
        /// The possible values are
        ///   1) REST
        ///   2) gRPC
        ///   3) PROXY
        /// </summary>
        public string ServiceType { get; set; }
        
        /// <summary>
        /// Initializes a new instance of configuration. The service type
        /// must be given as an argument.
        /// </summary>
        /// <param name="serviceType"></param>
        protected MockedServiceConfiguration(string serviceType)
        {
            if (string.IsNullOrWhiteSpace(serviceType))
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            ServiceType = serviceType;
        }

    }
}
