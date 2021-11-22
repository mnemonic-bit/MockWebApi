using MockWebApi.Configuration.Model;
using MockWebApi.Routing;
using MockWebApi.Tests.TestUtils;
using System;
using System.Linq;
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
        [InlineData("/some/path/{variables}/included?key1=123&key2=hello")]
        public void AddRoute_ShouldExtendTheGraph(string pathTemplate)
        {
            // Arrange
            IRouteParser routeParser = new RouteParser();
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>(routeParser);
            EndpointDescription endpointDescription = new EndpointDescription();

            // Act
            graphMatcher.AddRoute(pathTemplate, endpointDescription);

            // Assert
            //TODO:
        }

        [Theory]
        [InlineData("/{variable}/fixed/start", "/fixed/{variable}/start")]
        [InlineData("/{variable1}/{variable2}/start", "/{variable}/fixed/start")]
        [InlineData("/{variable1}/fixed/{variable2}", "/{variable}/fixed/start")]
        public void AddRoute_ShouldThrowException_WhenOverlappingRoutesAreGiven(string pathTemplate1, string pathTemplate2)
        {
            // Arrange
            IRouteParser routeParser = new RouteParser();
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>(routeParser);
            EndpointDescription endpointDescription = new EndpointDescription();
            graphMatcher.AddRoute(pathTemplate1, endpointDescription);

            // Act + Assert
            Assert.ThrowsAny<Exception>(() => graphMatcher.AddRoute(pathTemplate2, endpointDescription));
        }

        [Theory]
        [InlineData("/some/path", "/some/path")]
        [InlineData("/{variable1}/{variable2}/start", "/{variable1}/{variable2}/start")]
        [InlineData("/{variable}/fixed/start", "/{variable}/fixed/start")]
        public void AddRoute_ShouldReplaceInfoItem_WhenSamePathIsAddedWithNewInfos(string pathTemplate, string path)
        {
            // Arrange
            IRouteParser routeParser = new RouteParser();
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>(routeParser);

            EndpointDescription oldEndpointDescription = new EndpointDescription()
            {
                Route = pathTemplate,
                PersistRequestInformation = true
            };
            graphMatcher.AddRoute(pathTemplate, oldEndpointDescription);

            EndpointDescription newEndpointDescription = new EndpointDescription()
            {
                Route = pathTemplate,
                PersistRequestInformation = false
            };

            // Act
            graphMatcher.AddRoute(pathTemplate, newEndpointDescription);
            bool result = graphMatcher.TryFindRoute(path, out EndpointDescription info);

            // Assert
            Assert.True(result);
            Assert.NotNull(info);
            Assert.Equal(newEndpointDescription, info);
        }

        [Theory]
        [InlineData("/some/path", "/some/path")]
        [InlineData("/{variable1}/{variable2}/start", "/{variable1}/{variable2}/start")]
        [InlineData("/{variable}/fixed/start", "/{variable}/fixed/start")]
        public void DeleteRoute_ShouldRemoveInfoItem(string pathTemplate, string path)
        {
            // Arrange
            IRouteParser routeParser = new RouteParser();
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>(routeParser);

            EndpointDescription endpointDescription = new EndpointDescription()
            {
                Route = pathTemplate,
                PersistRequestInformation = true
            };

            // Act
            graphMatcher.AddRoute(pathTemplate, endpointDescription);
            bool removeResult = graphMatcher.Remove(pathTemplate);
            bool result = graphMatcher.TryFindRoute(path, out EndpointDescription info);

            // Assert
            Assert.True(removeResult);
            Assert.False(result);
            Assert.Null(info);
        }

        [Theory]
        [InlineData(new string[] { "/some/path/with/less/segments", "/some/path/with/more/segments" }, "/some/path")]
        [InlineData(new string[] { "/segment1/segment2/start" }, "/{variable1}/{variable2}/start")]
        public void DeleteRoute_ShouldLeaveOtherInfoItems_WhenShorterPathIsDeleted(string[] pathTemplates, string path)
        {
            // Arrange
            IRouteParser routeParser = new RouteParser();
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>(routeParser);

            foreach(string pathTemplate in pathTemplates)
            {
                graphMatcher.AddRoute(pathTemplate);
            }

            graphMatcher.AddRoute(path);

            // Act
            bool removeResult = graphMatcher.Remove(path);
            bool tryFindResult = pathTemplates.Select(p => graphMatcher.TryFindRoute(p, out EndpointDescription _)).All(b => b);

            // Assert
            Assert.True(removeResult);
            Assert.True(tryFindResult);
        }

        [Theory]
        [InlineData(new string[] { "/some/path/with/less/segments", "/some/path/with/more/segments", "/some/path" }, "/some/path/with/less/segments")]
        //[InlineData(new string[] { "/{variable1}/{variable2}/start", "/{variable}/fixed/start" }, "/different/fixed/start")]
        //[InlineData(new string[] { "/{variable1}/start/{variable2}", "/{variable}/start/fixed" }, "/different/start/fixed")]
        public void TryMatch_ShouldFindMatch_scratch_method(string[] pathTemplates, string path)
        {
            // Arrange
            IRouteParser routeParser = new RouteParser();
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>(routeParser);

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
            Assert.NotNull(routeMatch);
        }

        [Theory]
        [InlineData(new string[] { "/some/path" }, "/some/path")]
        [InlineData(new string[] { "/some/path/" }, "/some/path")]
        [InlineData(new string[] { "/some/other/path", "/some/path/" }, "/some/path")]
        [InlineData(new string[] { "/some/path/with/less/segments", "/some/path/with/more/segments", "/some/path" }, "/some/path")]
        [InlineData(new string[] { "/some/path", "/some/path/with/less/segments", "/some/path/with/more/segments" }, "/some/path")]
        [InlineData(new string[] { "/some/{variable}/path", "/some/different/path" }, "/some/different/path")]
        [InlineData(new string[] { "/some/{variable}/path", "/some/different/path" }, "/some/static/path")]
        [InlineData(new string[] { "/some/{variable}/path", "/some/{variable}/path/with/more/segments" }, "/some/different/path")]
        [InlineData(new string[] { "/some/{variable}/path", "/some/{variable}/path/with/more/segments" }, "/some/different/path/with/more/segments")]
        [InlineData(new string[] { "/{variable1}/{variable2}/start", "/{variable}/fixed/start" }, "/different/fixed/start")]
        [InlineData(new string[] { "/{variable1}/start/{variable2}", "/{variable}/start/fixed" }, "/different/start/fixed")]
        [InlineData(new string[] { "/{variable1}/{variable2}/start/end", "/{variable}/fixed/start" }, "/different/fixed/start")]
        [InlineData(new string[] { "/{variable1}/fixed/{variable2}", "/{variable}/fixed/start/end" }, "/different/fixed/start")]
        [InlineData(new string[] { "/{variable1}/{variable2}/start/end", "/{variable}/fixed/start" }, "/different/fixed/start/end")]
        [InlineData(new string[] { "/{variable1}/fixed/{variable2}", "/{variable}/fixed/start/end" }, "/different/fixed/start/end")]
        [InlineData(new string[] { "/some/specific/path", "/some/speficic/path/detail" }, "/some/specific/path")]
        [InlineData(new string[] { "/some/specific/path", "/some/speficic/path/detail" }, "/some/speficic/path/detail")]
        [InlineData(new string[] { "/some/specific/path?emptyParam=&paramWithValue=ABC", "/some/specific/path?emptyParam=&paramWithValue=XYZ" }, "/some/specific/path?emptyParam=&paramWithValue=ABC")]
        [InlineData(new string[] { "/some/specific/path?emptyParam=&paramWithValue=ABC", "/some/specific/path?emptyParam=&paramWithValue={var}" }, "/some/specific/path?emptyParam=&paramWithValue=A")]
        [InlineData(new string[] { "/some/path?var1={param1}&var2=value2" }, "/some/path?var1=some-value&var2=value2")]
        [InlineData(new string[] { "/some/path?var1={param1}&var2=value2" }, "/some/path?var2=value2")]
        public void TryMatch_ShouldFindMatch(string[] pathTemplates, string path)
        {
            // Arrange
            IRouteParser routeParser = new RouteParser();
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>(routeParser);

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
            Assert.NotNull(routeMatch);
        }

        [Theory]
        [InlineData(new string[] { "/some/path?var1=value1" }, "/some/path?var1=value1&var2=value2")]
        [InlineData(new string[] { "/some/path?var1=value1&var2=value2" }, "/some/path?var1=value1&var2=value2")]
        public void TryMatch_ShouldFindMatchWithParameters(string[] pathTemplates, string path)
        {
            // Arrange
            IRouteParser routeParser = new RouteParser();
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>(routeParser);

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
            Assert.NotNull(routeMatch);
        }

        [Theory]
        [InlineData(new string[] { "/some/other/path" }, "/some/path")]
        [InlineData(new string[] { "/some/specific/path", "/some/speficic/path/detail" }, "/some/specific")]
        [InlineData(new string[] { "/some/{variable}/path" }, "/some/path")]
        [InlineData(new string[] { "/some/other/path", "/some/path/but/longer" }, "/some/path")]
        [InlineData(new string[] { "/some/{variable}/path", "/some/{variable}/continued" }, "/some/path")]
        [InlineData(new string[] { "/some/path?var1=value1&var2=value2" }, "/some/path")]
        public void TryMatch_ShouldNotFindMatch(string[] pathTemplates, string path)
        {
            // Arrange
            IRouteParser routeParser = new RouteParser();
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>(routeParser);

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
            Assert.Null(routeMatch);
        }

        [Theory]
        [InlineData(new string[] { "/some/path" }, "/some/path")]
        [InlineData(new string[] { "/{variable1}/{variable2}/start", "/{variable}/fixed/start" }, "/{variable1}/{variable2}/start")]
        [InlineData(new string[] { "/{variable1}/{variable2}/start", "/{variable}/fixed/start" }, "/{variable}/fixed/start")]
        public void TryFindRoute_ShouldFindMatch(string[] pathTemplates, string path)
        {
            // Arrange
            IRouteParser routeParser = new RouteParser();
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>(routeParser);

            foreach (string pathTemplate in pathTemplates)
            {
                EndpointDescription endpointDescription = new EndpointDescription()
                {
                    Route = pathTemplate
                };
                graphMatcher.AddRoute(pathTemplate, endpointDescription);
            }

            // Act
            bool result = graphMatcher.TryFindRoute(path, out EndpointDescription info);

            // Assert
            Assert.True(result);
            Assert.NotNull(info);
        }

        [Theory]
        [InlineData("/some/path", "/some/path")]
        [InlineData("/{variable1}/{variable2}/start", "/{variable1}/{variable2}/start")]
        [InlineData("/{variable}/fixed/start", "/{variable}/fixed/start")]
        public void TryFindRoute_ShouldReturnInfoItem_WhenSameRouteIsSearchedFor(string pathTemplate, string path)
        {
            // Arrange
            IRouteParser routeParser = new RouteParser();
            RouteGraphMatcher<EndpointDescription> graphMatcher = new RouteGraphMatcher<EndpointDescription>(routeParser);

            EndpointDescription endpointDescription = new EndpointDescription()
            {
                Route = pathTemplate,
                PersistRequestInformation = true
            };
            graphMatcher.AddRoute(pathTemplate, endpointDescription);

            // Act
            bool result = graphMatcher.TryFindRoute(path, out EndpointDescription info);

            // Assert
            Assert.True(result);
            Assert.NotNull(info);
            Assert.Equal(endpointDescription, info);
        }

    }
}
