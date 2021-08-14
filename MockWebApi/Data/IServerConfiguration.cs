using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Data
{
    public interface IServerConfiguration
    {

        string this[string index] { get; set; }

        bool Contains(string key);

        T Get<T>(string key);

        void Set<T>(string key, T value);

    }
}
