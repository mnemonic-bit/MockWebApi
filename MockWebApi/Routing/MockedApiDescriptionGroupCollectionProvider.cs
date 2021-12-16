using Microsoft.AspNetCore.Mvc.ApiExplorer;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using System.Collections.Generic;
using System.Linq;

namespace MockWebApi.Routing
{
    public class MockedApiDescriptionGroupCollectionProvider : IApiDescriptionGroupCollectionProvider
    {

        private int _apiVersion;
        private IHostConfiguration _hostConfiguration;

        public ApiDescriptionGroupCollection ApiDescriptionGroups
        {
            get
            {
                IReadOnlyList<ApiDescriptionGroup> descriptionGroups = CreateApiDescriptionGroups();
                return new ApiDescriptionGroupCollection(descriptionGroups, _apiVersion);
            }
        }

        public MockedApiDescriptionGroupCollectionProvider(IHostConfiguration hostConfiguration)
        {
            _apiVersion = 1;
            _apiDescriptionGroups = new Dictionary<string, ApiDescriptionGroup>();
            _hostConfiguration = hostConfiguration;
        }

        private readonly IDictionary<string, ApiDescriptionGroup> _apiDescriptionGroups;

        public void Add(ApiDescriptionGroup apiDescriptionGroup)
        {
            _apiDescriptionGroups.Add(apiDescriptionGroup.GroupName, apiDescriptionGroup);
        }

        public void Remove(string groupName)
        {
            _apiDescriptionGroups.Remove(groupName);
        }

        public bool TryGet(string groupName, out ApiDescriptionGroup apiDescriptionGroup)
        {
            return _apiDescriptionGroups.TryGetValue(groupName, out apiDescriptionGroup);
        }

        private IReadOnlyList<ApiDescriptionGroup> CreateApiDescriptionGroups()
        {
            return _hostConfiguration.Configurations
                .Select(CreateApiDescriptionGroup)
                .ToList()
                .AsReadOnly();
        }

        private ApiDescriptionGroup CreateApiDescriptionGroup(IServiceConfiguration serviceConfiguration)
        {
            IReadOnlyList<ApiDescription> items = new List<ApiDescription>();

            string serviceName = serviceConfiguration.ServiceName;

            items = serviceConfiguration.RouteMatcher
                .GetAllRoutes()
                .Select(route => CreateApiDescription(serviceName, route))
                .ToList()
                .AsReadOnly();

            ApiDescriptionGroup apiDescriptionGroup = new ApiDescriptionGroup(serviceName, items);

            return apiDescriptionGroup;
        }

        private ApiDescription CreateApiDescription(string serviceName, EndpointDescription endpointDescription)
        {
            ApiDescription apiDescription = new ApiDescription();

            apiDescription.GroupName = serviceName;
            apiDescription.HttpMethod = "GET"; // TODO
            apiDescription.RelativePath = endpointDescription.Route;
            //TODO: add more information

            return apiDescription;
        }

    }
}
