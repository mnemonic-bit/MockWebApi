using System;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Service.Rest;

namespace MockWebApi.Extension
{
    public static class StringExtensions
    {

        public static T? ParseInto<T>(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static (string, string) SplitAt(this string str, int index)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str), $"No string instance is given to split.");
            }

            if (index < 0 || index > str.Length)
            {
                throw new ArgumentException(nameof(index), $"The position to split the given string, which is {index}, is outside the bounds of the string (0..{str.Length - 1}).");
            }

            return (str.Substring(0, index), str.Substring(index));
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static IServiceConfiguration DeserializeServiceConfiguration(this string config, string serviceName)
        {
            MockedServiceConfiguration mockedServiceConfiguration = config.DeserializeYaml<MockedServiceConfiguration>() ?? new MockedServiceConfiguration();
            mockedServiceConfiguration.ServiceName = serviceName;
            mockedServiceConfiguration.BaseUrl ??= DefaultValues.DEFAULT_MOCK_BASE_URL;

            IServiceConfiguration serviceConfiguration = new ServiceConfiguration(
                mockedServiceConfiguration.ServiceName,
                mockedServiceConfiguration.BaseUrl);

            ServiceConfigurationReader serviceConfigurationReader = new ServiceConfigurationReader(serviceConfiguration);
            serviceConfigurationReader.ConfigureService(mockedServiceConfiguration, true);

            return serviceConfiguration;
        }

        public static IServiceConfiguration DeserializeServerConfiguration(this string config)
        {
            //TODO: implement a converter for the full server configuration.
            throw new NotImplementedException();
        }

    }
}
