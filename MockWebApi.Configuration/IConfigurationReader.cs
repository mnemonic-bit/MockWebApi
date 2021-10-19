using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IConfigurationReader
    {

        MockedWebApiServiceConfiguration ReadConfiguration(string fileName);

        MockedWebApiServiceConfiguration ReadConfiguration(string configuration, string configurationFormat);

        MockedWebApiServiceConfiguration ReadFromJson(string text);

        MockedWebApiServiceConfiguration ReadFromYaml(string text);

    }
}
