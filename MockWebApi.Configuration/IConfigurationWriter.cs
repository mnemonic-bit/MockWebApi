using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IConfigurationWriter
    {

        string WriteConfiguration(MockedRestServiceConfiguration serviceConfiguration, string outputFormat = "YAML");

        string WriteConfiguration(MockedHostConfiguration hostConfiguration, string outputFormat = "YAML");

        string WriteToJson(MockedRestServiceConfiguration serviceConfiguration);

        string WriteToJson(MockedHostConfiguration hostConfiguration);

        string WriteToYaml(MockedRestServiceConfiguration serviceConfiguration);

        string WriteToYaml(MockedHostConfiguration serviceConfiguration);

    }
}
