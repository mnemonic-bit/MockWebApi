﻿using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IServiceConfigurationReader
    {

        public void ConfigureService(ServiceConfiguration configuration);

    }
}