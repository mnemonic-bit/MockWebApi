using MockWebApi.Configuration.Model;
using Newtonsoft.Json;
using System.IO;
using YamlDotNet.Serialization;

namespace MockWebApi.Configuration
{
    public class ConfigurationWriter : IConfigurationWriter
    {

        public ConfigurationWriter()
        {
        }

        public string WriteConfiguration(ServiceConfiguration serviceConfiguration, string outputFormat = "YAML")
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

        public string WriteToJson(ServiceConfiguration serviceConfiguration)
        {
            return SerializeToJson(serviceConfiguration);
        }

        public string WriteToYaml(ServiceConfiguration serviceConfiguration)
        {
            return SerializeToYaml(serviceConfiguration);
        }

        private string SerializeToJson<TObject>(TObject value)
        {
            string result = JsonConvert.SerializeObject(value, Formatting.Indented);
            return result;
        }

        private string SerializeToYaml<TObject>(TObject value)
        {
            StringWriter stringWriter = new StringWriter();
            Serializer serializer = new Serializer();
            serializer.Serialize(stringWriter, value);
            return stringWriter.ToString();
        }

    }
}
