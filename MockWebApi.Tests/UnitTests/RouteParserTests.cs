using MockWebApi.Routing;
using Xunit;

namespace MockWebApi.Tests.UnitTests
{
    public class RouteParserTests
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
        public void RouteParser_ShouldReturnsRoutes(string path)
        {
            // Arrange
            RouteParser routeParser = new RouteParser();

            // Act
            Route result = routeParser.Parse(path);

            // Assert

        }

    }
}
