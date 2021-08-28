using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IConfigurationReader
    {

        ServiceConfiguration ReadConfiguration(string fileName);

        ServiceConfiguration ReadFromJson(string text);

        ServiceConfiguration ReadFromYaml(string text);

    }
}
