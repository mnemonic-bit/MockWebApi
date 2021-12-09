using MockWebApi.Configuration.Extensions;
using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public class ConfigurationWriter : IConfigurationWriter
    {

        public ConfigurationWriter()
        {
        }

        public string WriteConfiguration(MockedWebApiServiceConfiguration serviceConfiguration, string outputFormat = "YAML")
        {
            switch (outputFormat.ToUpper())
            {
                case "JSON":
                    {
                        return WriteToJson(serviceConfiguration);
                    }
                case "CS":
                    {
                        return "This format is not implemented yet.";
                    }
                case "YAML":
                default:
                    {
                        return WriteToYaml(serviceConfiguration);
                    }
            }
        }

        public string WriteToJson(MockedWebApiServiceConfiguration serviceConfiguration)
        {
            return serviceConfiguration.SerializeToJson();
        }

        public string WriteToYaml(MockedWebApiServiceConfiguration serviceConfiguration)
        {
            return serviceConfiguration.SerializeToYaml();
        }

    }
}
