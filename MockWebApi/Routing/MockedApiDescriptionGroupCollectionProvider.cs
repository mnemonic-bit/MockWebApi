using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;

namespace MockWebApi.Routing
{
    public class MockedApiDescriptionGroupCollectionProvider : IApiDescriptionGroupCollectionProvider
    {

        private readonly int _apiVersion;
        private readonly IHostConfiguration _hostConfiguration;

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
            if (apiDescriptionGroup.GroupName == null)
            {
                throw new ArgumentNullException(nameof(apiDescriptionGroup), $"Internal error");
            }

            _apiDescriptionGroups.Add(apiDescriptionGroup.GroupName, apiDescriptionGroup);
        }

        public void Remove(string groupName)
        {
            _apiDescriptionGroups.Remove(groupName);
        }

        public bool TryGet(string groupName, out ApiDescriptionGroup? apiDescriptionGroup)
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
                .Select(endpointDescription => CreateApiDescription(serviceName, endpointDescription))
                .ToList()
                .AsReadOnly();

            ApiDescriptionGroup apiDescriptionGroup = new ApiDescriptionGroup(serviceName, items);

            return apiDescriptionGroup;
        }

        private ApiDescription CreateApiDescription(string serviceName, EndpointDescription endpointDescription)
        {
            ApiDescription apiDescription = new ApiDescription();

            apiDescription.ActionDescriptor = CreateActionDescriptor(endpointDescription);
            apiDescription.GroupName = null;// serviceName;
            apiDescription.HttpMethod = "GET";// endpointDescription.HttpMethod;
            apiDescription.RelativePath = endpointDescription.Route.TrimStart('/');

            return apiDescription;
        }

        private ActionDescriptor CreateActionDescriptor(EndpointDescription endpointDescription)
        {
            ActionDescriptor actionDescriptor = new ActionDescriptor()
            {
                ActionConstraints = new List<IActionConstraintMetadata>(),
                AttributeRouteInfo = new AttributeRouteInfo() { Template = endpointDescription.Route },
                BoundProperties = new List<ParameterDescriptor>(),
                DisplayName = endpointDescription.ToString(),
                EndpointMetadata = new List<object>(),
                FilterDescriptors = new List<FilterDescriptor>(),
                Parameters = new List<ParameterDescriptor>(),
                Properties = new Dictionary<object, object?>(),
                RouteValues = new Dictionary<string, string?>()
                {
                    { "action", "the-action-name" },
                    { "controller", "the-controller-name" } // This will be the header for the group of API methods
                }
            };

            return actionDescriptor;
        }
    }
}
