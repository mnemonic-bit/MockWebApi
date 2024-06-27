using System;
using System.Linq;
using MockWebApi.Configuration.Model;
using MockWebApi.Routing;

namespace MockWebApi.Configuration
{
    /// <summary>
    /// The <code>ConfigurationModelProvider</code> helps transform between the configuration
    /// model as it is known to the public API, and the configuration model which is used
    /// internally by the components in this API itself. This provider has methods for both
    /// directions, that is, from the public model to the internal model, and also from the
    /// internal model back to the external configuration model.
    /// </summary>
    public class ConfigurationModelProvider
    {

        public static MockedServiceConfiguration Transform(IServiceConfiguration configuration)
        {
            switch(configuration)
            {
                case IProxyServiceConfiguration proxyServiceConfiguration:
                    return TransformProxyConfiguration(proxyServiceConfiguration);
                case IRestServiceConfiguration restServiceConfiguration:
                    return TransformRestConfiguration(restServiceConfiguration);
                default:
                    throw new NotImplementedException($"The configuration type '{configuration.GetType().Name}' is not supported by this component.");
            }
        }

        public static IServiceConfiguration Transform(MockedServiceConfiguration configuration)
        {
            switch (configuration)
            {
                case MockedRestServiceConfiguration restServiceConfiguration:
                    return TransformRestConfiguration(restServiceConfiguration);
                case MockedProxyServiceConfiguration proxyServiceConfiguration:
                    return TransformProxyConfiguration(proxyServiceConfiguration);
                default:
                    throw new NotImplementedException($"The configuration type '{configuration.GetType().Name}' is not supported by this component.");
            }
        }


        private static MockedRestServiceConfiguration TransformRestConfiguration(IRestServiceConfiguration configuration)
        {
            var result = new MockedRestServiceConfiguration()
            {
                BaseUrl = configuration.Url,
                DefaultEndpointDescription = configuration.DefaultEndpointDescription,
                EndpointDescriptions = configuration.RouteMatcher
                    .GetAllRoutes()
                    .Select(s => s.EndpointDescription)
                    .ToArray(),
                JwtServiceOptions = configuration.JwtServiceOptions,
                ServiceName = configuration.ServiceName,
            };

            return result;
        }

        private static IRestServiceConfiguration TransformRestConfiguration(MockedRestServiceConfiguration configuration)
        {
            var result = new RestServiceConfiguration(configuration.ServiceName, configuration.BaseUrl)
            {
                DefaultEndpointDescription = configuration.DefaultEndpointDescription,
            };


            return result;
        }

        private static MockedProxyServiceConfiguration TransformProxyConfiguration(IProxyServiceConfiguration configuration)
        {
            var result = new MockedProxyServiceConfiguration()
            {
                //BaseUrl = "TODO",
                ServiceName = configuration.ServiceName,
            };

            return result;
        }

        private static IProxyServiceConfiguration TransformProxyConfiguration(MockedProxyServiceConfiguration configuration)
        {
            var result = new ProxyServiceConfiguration(configuration.ServiceName, configuration.BaseUrl)
            {

            };

            return result;
        }

    }
}
