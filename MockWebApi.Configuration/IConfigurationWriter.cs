using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IConfigurationWriter
    {

        string WriteConfiguration(MockedWebApiServiceConfiguration serviceConfiguration, string outputFormat = "YAML");

        string WriteToJson(MockedWebApiServiceConfiguration serviceConfiguration);

        string WriteToYaml(MockedWebApiServiceConfiguration serviceConfiguration);

    }
}
