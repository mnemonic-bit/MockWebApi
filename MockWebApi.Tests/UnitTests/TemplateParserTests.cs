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
            string text = "some {{ var1 }} templated {{ var2 }} text";
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
            Assert.Equal(fragments[0], template.Fragments[0]);
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

        [Theory]
        [InlineData(@"{""node"": { ""child"": 123 } }")]
        public void Parse_ShouldReturnsStringFragment_WhenTextIsJson(string text)
        {
            // Arrange
            TemplateParser parser = new TemplateParser();
            Fragment[] fragments = new Fragment[]
            {
                new StringFragment(text)
            };

            // Act
            Template template = parser.Parse(text);

            // Assert
            Assert.Equal(fragments, template.Fragments);
        }

        [Fact]
        public void Parse_ShouldReturnFragments_WhenTextIsJsonWithScripting()
        {
            // Arrange
            string text = @"{""node"": { ""child"": {{var1}} } }";

            Fragment[] fragments = new Fragment[]
            {
                new StringFragment(@"{""node"": { ""child"": "),
                new ScriptFragment("var1"),
                new StringFragment(@" } }")
            };

            TemplateParser parser = new TemplateParser();

            // Act
            Template template = parser.Parse(text);

            // Assert
            Assert.Equal(fragments, template.Fragments);
        }

    }
}
