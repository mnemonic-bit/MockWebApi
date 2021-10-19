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

        public MockedWebApiServiceConfiguration ReadConfiguration(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }

            string fileExtension = GetFileExtension(fileName);

            string fileContents = File.ReadAllText(fileName);

            return ReadConfiguration(fileContents, fileExtension.ToUpper());
        }

        public MockedWebApiServiceConfiguration ReadConfiguration(string configuration, string configurationFormat)
        {
            switch (configurationFormat)
            {
                case "JSON":
                    {
                        return ReadFromJson(configuration);
                    }
                case "YAML":
                default:
                    {
                        return ReadFromYaml(configuration);
                    }
            }
        }

        public MockedWebApiServiceConfiguration ReadFromJson(string text)
        {
            MockedWebApiServiceConfiguration configuration = DeserializeJson<MockedWebApiServiceConfiguration>(text);
            return configuration;
        }

        public MockedWebApiServiceConfiguration ReadFromYaml(string text)
        {
            MockedWebApiServiceConfiguration config = DeserializeYaml<MockedWebApiServiceConfiguration>(text);
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
