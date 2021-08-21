﻿using MockWebApi.Model;
using MockWebApi.Routing;
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
        [InlineData(new string[] { "/some/path" }, "/some/path")]
        [InlineData(new string[] { "/some/path/" }, "/some/path")]
        [InlineData(new string[] { "/some/path?var1=value1&var2=value2" }, "/some/path?var1=value1&var2=value2")]
        [InlineData(new string[] { "/some/path?var1={param1}&var2=value2" }, "/some/path?var1=some-value&var2=value2")]
        [InlineData(new string[] { "/some/other/path", "/some/path/" }, "/some/path")]
        [InlineData(new string[] { "/some/{variable}/path", "/some/different/path" }, "/some/other/path")]
        //[InlineData(new string[] { "/some/{variable}/path", "/some/different/path" }, "/some/different/path")] //TODO: matches the most specific (literal) one
        public void TryMatch_ShouldFindMatch(string[] pathTemplates, string path)
        {
            // Arrange
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>();

            foreach (string pathTemplate in pathTemplates)
            {
                EndpointDescription endpointDescription = new EndpointDescription()
                {
                    Route = pathTemplate
                };
                graphMatcher.AddRoute(pathTemplate, endpointDescription);
            }

            // Act
            bool result = graphMatcher.TryMatch(path, out RouteMatch<EndpointDescription> routeMatch);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(new string[] { "/some/other/path" }, "/some/path")]
        [InlineData(new string[] { "/some/{variable}/path" }, "/some/path")]
        [InlineData(new string[] { "/some/other/path", "/some/path/but/longer" }, "/some/path")]
        [InlineData(new string[] { "/some/{variable}/path", "/some/{variable}/continued" }, "/some/path")]
        [InlineData(new string[] { "/some/path?var1=value1&var2=value2" }, "/some/path")]
        public void TryMatch_ShouldNotFindMatch(string[] pathTemplates, string path)
        {
            // Arrange
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>();

            foreach (string pathTemplate in pathTemplates)
            {
                EndpointDescription endpointDescription = new EndpointDescription()
                {
                    Route = pathTemplate
                };
                graphMatcher.AddRoute(pathTemplate, endpointDescription);
            }

            // Act
            bool result = graphMatcher.TryMatch(path, out RouteMatch<EndpointDescription> routeMatch);

            // Assert
            Assert.False(result);
        }

    }
}
