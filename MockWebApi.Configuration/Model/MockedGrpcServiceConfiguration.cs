using System;
using System.Collections.Generic;
using System.Text;

namespace MockWebApi.Configuration.Model
{
    public class MockedGrpcServiceConfiguration : MockedServiceConfiguration
    {

        public MockedGrpcServiceConfiguration()
            : base("GRPC")
        {
        }

    }
}
