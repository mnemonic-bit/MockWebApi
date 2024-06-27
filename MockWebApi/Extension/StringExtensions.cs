using System;
using System.Diagnostics.CodeAnalysis;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;

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

        public static void DeserializeServiceConfiguration(this string config, string serviceName, [NotNull] ref IServiceConfiguration? serviceConfiguration)
        {
            MockedServiceConfiguration mockedServiceConfiguration = ConfigurationProvider.Deserialize(config);

            // By default we provide a mocked REST service.
            //TODO: this might not be a good place where and how the default service
            // type should be encoded. Please redesign this.
            if (mockedServiceConfiguration == null)
            {
                mockedServiceConfiguration = new MockedRestServiceConfiguration();
                mockedServiceConfiguration.ServiceName = serviceName;
                mockedServiceConfiguration.BaseUrl ??= DefaultValues.DEFAULT_MOCK_BASE_URL;
            }

            ServiceConfigurationReader serviceConfigurationReader = new ServiceConfigurationReader();
            serviceConfigurationReader.Load(mockedServiceConfiguration, ref serviceConfiguration);
        }

    }
}
