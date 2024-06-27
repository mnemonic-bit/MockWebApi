using System;
using System.Collections.Generic;
using System.Text;

namespace MockWebApi.Configuration.Model
{
    /// <summary>
    /// Defines the configuration of a mocked proxy service.
    /// </summary>
    public class MockedProxyServiceConfiguration : MockedServiceConfiguration
    {

        public string DestinationUrl { get; set; }

        public MockedProxyServiceConfiguration()
            : base("PROXY")
        {
            DestinationUrl = string.Empty;
        }

    }
}
