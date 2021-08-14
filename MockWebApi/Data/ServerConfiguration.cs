using MockWebApi.Extension;
using System;
using System.Collections.Generic;

namespace MockWebApi.Data
{
    public class ServerConfiguration : IServerConfiguration
    {

        private readonly Dictionary<string, string> _config;

        public ServerConfiguration()
        {
            _config = new Dictionary<string, string>();
            InitServerConfiguration();
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

        private void InitServerConfiguration()
        {
            Set(Parameters.TrackServiceApiCalls, false);
            Set(Parameters.DefaultHttpStatusCode, 200);
            Set(Parameters.DefaultContentType, "text/plain");
        }

        public static class Parameters
        {
            public static readonly string DefaultHttpStatusCode = "DefaultHttpStatusCode";
            public static readonly string TrackServiceApiCalls = "TrackServiceApiCalls";
            public static readonly string DefaultContentType = "DefaultContentType";
        }

    }
}
