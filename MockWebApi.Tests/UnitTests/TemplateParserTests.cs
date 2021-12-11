using MockWebApi.Templating;
using Xunit;

namespace MockWebApi.Tests.UnitTests
{
    public class TemplateParserTests
    {

        [Theory]
        [InlineData("Hello!")]
        [InlineData("Hello World!")]
        [InlineData("Hello\nWorld!")]
        public void Parse_ShouldReturnOnlyOneFragment_WhenOnlyTextIsParsed(string text)
        {
            // Arrange
            TemplateParser parser = new TemplateParser();

            // Act
            Template template = parser.Parse(text);

            // Assert
            Assert.Single(template.Fragments);

            Fragment fragment = template.Fragments[0];
            Assert.IsType<StringFragment>(template.Fragments[0]);

            StringFragment stringFragment = fragment as StringFragment;
            Assert.Equal(text, stringFragment.Text);
        }

        [Fact]
        public void Parse_ShouldReturnFormatString_WhenTextHasMarks()
        {
            // Arrange
            string text = "some {{ var1 }} teplated {{ var2 }} text";
            Fragment[] fragments = new Fragment[]
            { 
                new StringFragment("some "),
                new ScriptFragment("var1"),
                new StringFragment(" templated "),
                new ScriptFragment("var2"),
                new StringFragment(" text")
            };

            TemplateParser parser = new TemplateParser();

            // Act
            Template template = parser.Parse(text);

            // Assert
            Assert.Equal(fragments, template.Fragments);
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
