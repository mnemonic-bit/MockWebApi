using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Collections.Generic;
using System.Linq;

namespace MockWebApi.Routing
{
    public class MockedApiDescriptionGroupCollectionProvider : IApiDescriptionGroupCollectionProvider
    {
        
        public ApiDescriptionGroupCollection ApiDescriptionGroups
        {
            get
            {
                IReadOnlyList<ApiDescriptionGroup> descriptionGroups = _apiDescriptionGroups
                    .Values
                    .ToList()
                    .AsReadOnly();

                return new ApiDescriptionGroupCollection(descriptionGroups, 1);
            }
        }

        public MockedApiDescriptionGroupCollectionProvider()
        {
            _apiDescriptionGroups = new Dictionary<string, ApiDescriptionGroup>();
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

    }
}
