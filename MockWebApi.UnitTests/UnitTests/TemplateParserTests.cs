using FluentAssertions;
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
            template.Fragments.Should().HaveCount(1);

            Fragment fragment = template.Fragments[0];
            fragment.Should().NotBeNull();
            fragment.Should().BeOfType<StringFragment>();

            StringFragment stringFragment = fragment as StringFragment;
            stringFragment.Should().NotBeNull();
            stringFragment.Text.Should().Be(text);
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
            template.Fragments[0].Should().NotBeNull();
            template.Fragments[0].Should().Be(fragments[0]);
            template.Fragments.Should().BeEquivalentTo(fragments);
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
