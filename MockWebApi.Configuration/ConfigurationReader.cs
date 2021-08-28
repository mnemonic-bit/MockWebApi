using MockWebApi.Configuration.Model;
using Newtonsoft.Json;
using System.IO;
using YamlDotNet.Serialization;

namespace MockWebApi.Configuration
{
    public class ConfigurationReader : IConfigurationReader
    {

        public ConfigurationReader()
        {
        }

        public ServiceConfiguration ReadConfiguration(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }

            string fileExtension = GetFileExtension(fileName);

            string fileContents = File.ReadAllText(fileName);

            switch (fileExtension.ToUpper())
            {
                case "JSON":
                    {
                        return ReadFromJson(fileContents);
                    }
                case "YAML":
                default:
                    {
                        return ReadFromYaml(fileContents);
                    }
            }
        }

        public ServiceConfiguration ReadFromJson(string text)
        {
            ServiceConfiguration configuration = DeserializeJson<ServiceConfiguration>(text);
            return configuration;
        }

        public ServiceConfiguration ReadFromYaml(string text)
        {
            ServiceConfiguration config = DeserializeYaml<ServiceConfiguration>(text);
            return config;
        }

        private T DeserializeYaml<T>(string yamlText)
        {
            IDeserializer deserializer = new DeserializerBuilder()
                .Build();

            return deserializer.Deserialize<T>(yamlText);
        }

        private T DeserializeJson<T>(string jsonText)
        {
            T result = JsonConvert.DeserializeObject<T>(jsonText);
            return result;
        }

        private string GetFileExtension(string fileName)
        {
            int indexOfTheDot = fileName.LastIndexOf('.');

            if (indexOfTheDot < 0)
            {
                return null;
            }

            return fileName.Substring(indexOfTheDot, fileName.Length - indexOfTheDot - 1);
        }

    }
}
