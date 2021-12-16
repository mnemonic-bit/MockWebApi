using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IConfigurationFileWriter
    {

        string WriteConfiguration(MockedServiceConfiguration serviceConfiguration, string outputFormat = "YAML");

        string WriteConfiguration(MockedHostConfiguration hostConfiguration, string outputFormat = "YAML");

        string WriteToJson(MockedServiceConfiguration serviceConfiguration);

        string WriteToJson(MockedHostConfiguration hostConfiguration);

        string WriteToYaml(MockedServiceConfiguration serviceConfiguration);

        string WriteToYaml(MockedHostConfiguration serviceConfiguration);

    }
}
