using MockWebApi.Extension;
using System.Collections.Generic;
using System.Linq;

namespace MockWebApi.Data
{
    public class ConfigurationCollection : IConfigurationCollection
    {

        private readonly Dictionary<string, string> _config;

        public ConfigurationCollection()
        {
            _config = new Dictionary<string, string>();
        }

        public string this[string index]
        {
            get => _config[index];
            set => _config[index] = value;
        }

        public bool Contains(string key)
        {
            return _config.ContainsKey(key);
        }

        public T Get<T>(string key)
        {
            if (!Contains(key))
            {
                return default(T);
            }

            string value = _config[key];
            return value.ParseInto<T>();
        }

        public void Set<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (value == null)
            {
                return;
            }

            _config[key] = value.ToString();
        }

        public override string ToString()
        {
            IEnumerable<string> configItems = _config.Select(item => $"{item.Key}: {item.Value}");

            string result = string.Join("\n", configItems);

            return result;
        }

        //TODO: move the definitions out of this class
        public static class Parameters
        {
            public static readonly string DefaultHttpStatusCode = "DefaultHttpStatusCode";
            public static readonly string TrackServiceApiCalls = "TrackServiceApiCalls";
            public static readonly string LogServiceApiCalls = "LogServiceApiCalls";
            public static readonly string DefaultContentType = "DefaultContentType";
            public static readonly string MeasureRequestTimings = "MeasureRequestTimings";
        }

    }
}
