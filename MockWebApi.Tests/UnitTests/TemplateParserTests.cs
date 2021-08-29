using MockWebApi.Templating;
using Xunit;

namespace MockWebApi.Tests.UnitTests
{
    public class TemplateParserTests
    {

        [Theory]
        [InlineData("skldfj sldkjf sdlkfj sfd\n sldkfjslkdfjl")]
        [InlineData("some useful {text}\n but with no teplate pattern inside")]
        [InlineData("some useful {text\n but with no teplate} pattern inside")]
        public void Parse_ShouldReturnTheSameText_WhenNoTemplatingMarksAreMissing(string text)
        {
            // Arrange
            //using Microsoft.AspNetCore.Routing.Template;
            //TemplateParser parser = new TemplateParser();
            TemplateParser parser = new TemplateParser();

            // Act
            Template template = parser.Parse(text);

            // Assert
            Assert.Equal(text, template.FormatString);
            Assert.Empty(template.Parameters);
        }

        [Theory]
        [InlineData("some {{ var1 }} teplated {{ var2 }} text", "some {0} teplated {1} text", new string[] { "var1", "var2" })]
        [InlineData("some {{ if (expr) { stmt; } }} teplated {{ Guid.NewGuid().ToString() }} text", "some {0} teplated {1} text", new string[] { "if (expr) { stmt; }", "Guid.NewGuid().ToString()" })]
        public void Parse_ShouldReturnFormatString_WhenTextHasMarks(string text, string templateText, string[] parametes)
        {
            // Arrange
            TemplateParser parser = new TemplateParser();

            // Act
            Template template = parser.Parse(text);

            // Assert
            Assert.Equal(templateText, template.FormatString);
            Assert.Equal(parametes, template.Parameters);
        }

        [Theory]
        [InlineData("some {{ var1 }} teplated {{ var2 text")]
        public void Parse_ShouldThrowException_WhenBracesAreImbalanced(string text)
        {
            // Arrange
            TemplateParser parser = new TemplateParser();

            // Act + Assert
            Assert.Throws<MalformedTemplateException>(() => parser.Parse(text));
        }

    }
}
