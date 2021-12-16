using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IServiceConfigurationFileReader
    {

        MockedServiceConfiguration ReadConfiguration(string fileName);

        MockedServiceConfiguration ReadConfiguration(string configuration, string configurationFormat);

        MockedServiceConfiguration ReadFromJson(string text);

        MockedServiceConfiguration ReadFromYaml(string text);

    }
}
