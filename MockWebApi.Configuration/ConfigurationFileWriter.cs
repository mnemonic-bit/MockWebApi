using MockWebApi.Configuration.Extensions;
using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public class ConfigurationFileWriter : IConfigurationFileWriter
    {

        public ConfigurationFileWriter()
        {
        }

        public string WriteConfiguration(MockedServiceConfiguration serviceConfiguration, string outputFormat = "YAML")
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

        public string WriteConfiguration(MockedHostConfiguration hostConfiguration, string outputFormat = "YAML")
        {
            switch (outputFormat.ToUpper())
            {
                case "JSON":
                    {
                        return WriteToJson(hostConfiguration);
                    }
                case "CS":
                    {
                        return "This format is not implemented yet.";
                    }
                case "YAML":
                default:
                    {
                        return WriteToYaml(hostConfiguration);
                    }
            }
        }

        public string WriteToJson(MockedServiceConfiguration serviceConfiguration)
        {
            return serviceConfiguration.SerializeToJson();
        }

        public string WriteToJson(MockedHostConfiguration hostConfiguration)
        {
            return hostConfiguration.SerializeToJson();
        }

        public string WriteToYaml(MockedServiceConfiguration serviceConfiguration)
        {
            return serviceConfiguration.SerializeToYaml();
        }

        public string WriteToYaml(MockedHostConfiguration hostConfiguration)
        {
            return hostConfiguration.SerializeToYaml();
        }

    }
}
