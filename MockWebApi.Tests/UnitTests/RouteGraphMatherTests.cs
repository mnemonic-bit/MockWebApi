using MockWebApi.Model;
using MockWebApi.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MockWebApi.Tests.UnitTests
{
    public class RouteGraphMatherTests
    {

        [Theory]
        [InlineData("/some/path")]
        [InlineData("/some/path/")]
        [InlineData("/some/{variable}/path")]
        [InlineData("/some/path?key1=123")]
        [InlineData("/some/path/?key1=123")]
        [InlineData("/some/path?key1=123&key2=hello")]
        [InlineData("/page1?id=3&format=yaml&content-type=text/plain")]
        [InlineData("/page1?id=3123&format=json&action=edit&text=It's%20a%20brave%20new%20world!")]
        public void AddRoute_ShouldExtensTheGraph(string pathTemplate)
        {
            // Arrange
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>();
            EndpointDescription endpointDescription = new EndpointDescription();

            // Act
            graphMatcher.AddRoute(pathTemplate, endpointDescription);

            // Assert

        }

        [Theory]
        [InlineData("/some/path", "/some/path")]
        [InlineData("/some/path/", "/some/path")]
        [InlineData("/some/{variable}/path", "/some/other/path")]
        public void TryMatch_ShouldExtensTheGraph(string pathTemplate, string path)
        {
            // Arrange
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>();
            EndpointDescription endpointDescription = new EndpointDescription();
            graphMatcher.AddRoute(pathTemplate, endpointDescription);

            // Act
            bool result = graphMatcher.TryMatch(path, out EndpointDescription routeInfo);

            // Assert

        }

    }
}
