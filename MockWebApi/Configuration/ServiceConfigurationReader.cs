using System;
using System.Diagnostics.CodeAnalysis;
using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public class ServiceConfigurationReader : IServiceConfigurationReader
    {

        public ServiceConfigurationReader()
        {
        }

        public void Load(MockedServiceConfiguration configuration, [NotNull] ref IServiceConfiguration? serviceConfiguration)
        {
            switch(configuration.ServiceType.ToUpper())
            {
                case "GRPC":
                    LoadGrpcServiceConfiguration(configuration, ref serviceConfiguration);
                    break;
                case "PROXY":
                    LoadProxyServiceConfiguration(configuration, ref serviceConfiguration);
                    break;
                case "REST":
                    LoadRestServiceConfiguration(configuration, ref serviceConfiguration);
                    break;
                default:
                    throw new NotImplementedException($"Internal error: The service type '{configuration.ServiceType}' is not supported by this component.");
            }
        }


        private void LoadGrpcServiceConfiguration(MockedServiceConfiguration configuration, [NotNull] ref IServiceConfiguration? serviceConfiguration)
        {
            if (configuration is not MockedGrpcServiceConfiguration grpcConfiguration)
            {
                throw new ArgumentException($"The given configuration is not of the required type {nameof(MockedRestServiceConfiguration)}.", nameof(grpcConfiguration));
            }

            throw new NotImplementedException();
        }

        private void LoadProxyServiceConfiguration(MockedServiceConfiguration configuration, [NotNull] ref IServiceConfiguration? serviceConfiguration)
        {
            if (configuration is not MockedProxyServiceConfiguration proxyConfiguration)
            {
                throw new ArgumentException($"The given configuration is not of the required type {nameof(MockedRestServiceConfiguration)}.", nameof(proxyConfiguration));
            }

            serviceConfiguration ??= new ProxyServiceConfiguration(configuration.ServiceName, configuration.BaseUrl)
            {
                DestinationUrl = proxyConfiguration.DestinationUrl,
            };
        }

        private void LoadRestServiceConfiguration(MockedServiceConfiguration configuration, [NotNull] ref IServiceConfiguration? serviceConfiguration)
        {
            if (configuration is not MockedRestServiceConfiguration restConfiguration)
            {
                throw new ArgumentException($"The given configuration is not of the required type {nameof(MockedRestServiceConfiguration)}.", nameof(restConfiguration));
            }

            if (serviceConfiguration != null && serviceConfiguration is not IRestServiceConfiguration)
            {
                throw new ArgumentException($"The given configuration is not of the required type {nameof(IRestServiceConfiguration)}.", nameof(serviceConfiguration));
            }

            bool overwriteExisting = serviceConfiguration != null;

            IRestServiceConfiguration restServiceConfiguration =
                (serviceConfiguration as IRestServiceConfiguration)
                ?? new RestServiceConfiguration(
                    restConfiguration.ServiceName,
                    restConfiguration.BaseUrl);

            serviceConfiguration = restServiceConfiguration;

            if (overwriteExisting)
            {
                restServiceConfiguration.DefaultEndpointDescription = restConfiguration.DefaultEndpointDescription;
                restServiceConfiguration.JwtServiceOptions = restConfiguration.JwtServiceOptions;
            }
            else
            {
                restServiceConfiguration.DefaultEndpointDescription ??= restConfiguration.DefaultEndpointDescription;
                restServiceConfiguration.JwtServiceOptions ??= restConfiguration.JwtServiceOptions;
            }

            if (overwriteExisting)
            {
                restServiceConfiguration.RouteMatcher.RemoveAll();
                foreach (EndpointDescription endpointDescription in (restConfiguration.EndpointDescriptions ?? new EndpointDescription[] { }))
                {
                    restServiceConfiguration.RouteMatcher.AddRoute(endpointDescription.Route, new EndpointState(endpointDescription));
                }
            }
            else
            {
                foreach (EndpointDescription endpoint in restConfiguration.EndpointDescriptions ?? new EndpointDescription[] { })
                {
                    restServiceConfiguration.RouteMatcher.AddRoute(endpoint.Route, new EndpointState(endpoint));
                }
            }
        }

    }
}
