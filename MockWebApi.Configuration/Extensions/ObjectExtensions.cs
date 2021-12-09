using Newtonsoft.Json;
using System.IO;
using YamlDotNet.Serialization;

namespace MockWebApi.Configuration.Extensions
{
    public static class ObjectExtensions
    {

        public static string SerializeToJson<TObject>(this TObject obj)
        {
            string result = JsonConvert.SerializeObject(obj, Formatting.Indented);
            return result;
        }

        public static string SerializeToYaml<TObject>(this TObject obj)
        {
            StringWriter stringWriter = new StringWriter();
            Serializer serializer = new Serializer();
            serializer.Serialize(stringWriter, obj);
            return stringWriter.ToString();
        }

        public static string Serialize<TObject>(this TObject obj, string format = "YAML") // TODO: change the format parameter from string to enum
        {
            switch (format)
            {
                case "YAML":
                    return obj.SerializeToYaml<TObject>();
                case "JSON":
                    return obj.SerializeToJson<TObject>();
                default:
                    return string.Empty;
            }
        }


    }
}
