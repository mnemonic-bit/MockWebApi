using Microsoft.AspNetCore.Routing.Template;
using Xunit;

namespace MockWebApi.Tests.UnitTests
{
    public class MsRoutingTests
    {

        [Fact]
        public void Test()
        {
            // Arrange
            string template = "/user/{match}/static/value";

            // Act
            RouteTemplate routeTemplate = TemplateParser.Parse(template);
            
            // Assert

        }

    }
}
