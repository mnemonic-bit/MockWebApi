using MockWebApi.Templating;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MockWebApi.Tests.UnitTests
{
    public class TemplateExecutorTests
    {

        public static IEnumerable<object[]> ExecuteExampes()
        {
            yield return new object[] { "some {{ var1 }} teplated {{ var2 }} text", new Dictionary<string, string>(){ } };
            yield return new object[] { "some {{ if (expr) { stmt; } }} teplated {{ Guid.NewGuid().ToString() }} text", new Dictionary<string, string>() { } };
        }

        [Theory]
        [MemberData(nameof(ExecuteExampes))]
        public async Task Execute_ShouldReturnTheSameText_WhenNoTemplatingMarksAreMissing(string text, Dictionary<string, string> variables)
        {
            // Arrange
            TemplateParser parser = new TemplateParser();
            TemplateExecutor executor = new TemplateExecutor(parser);

            // Act
            string result = await executor.Execute(text, variables);

            // Assert

        }

    }
}
