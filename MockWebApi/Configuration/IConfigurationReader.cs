using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Configuration
{
    public interface IConfigurationReader
    {

        public ServiceConfiguration ReadFromJson(string text);

        public ServiceConfiguration ReadFromYaml(string text);

        public void ConfigureService(ServiceConfiguration configuration);

    }
}
