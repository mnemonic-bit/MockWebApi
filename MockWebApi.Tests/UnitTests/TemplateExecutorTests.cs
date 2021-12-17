using System.Collections.Generic;
using System.Threading.Tasks;

using MockWebApi.Templating;

using Xunit;

namespace MockWebApi.Tests.UnitTests
{
    public class TemplateExecutorTests
    {

        public static IEnumerable<object[]> FixedReplacementExamples()
        {
            yield return new object[] { "{{ var1 }}", new Dictionary<string, string>() { ["var1"] = "RES1" }, "RES1" };
            yield return new object[] { "{{ Console.Write(var1) }}", new Dictionary<string, string>() { ["var1"] = "RES1" }, "RES1" };
            yield return new object[] { "{{ if (expr == \"TEST\") { Console.Write(stmt); } }}", new Dictionary<string, string>() { ["expr"] = "TEST", ["stmt"] = "RES" }, "RES" };
            yield return new object[] { "some {{ var1 }} teplated {{ var2 }} text", new Dictionary<string, string>() { ["var1"] = "RES1", ["var2"] = "RES2" }, "some RES1 teplated RES2 text" };
            yield return new object[] { "some {{ Console.Write(var1) }} teplated {{ Console.Write(var2) }} text", new Dictionary<string, string>() { ["var1"] = "RES1", ["var2"] = "RES2" }, "some RES1 teplated RES2 text" };
        }

        [Theory]
        [MemberData(nameof(FixedReplacementExamples))]
        public async Task Execute_ShouldReturnTheSameText_WhenNoTemplatingMarksAreMissing(string text, Dictionary<string, string> variables, string expectedResult)
        {
            // Arrange
            TemplateParser parser = new TemplateParser();
            TemplateExecutor executor = new TemplateExecutor(parser);

            // Act
            string result = await executor.Execute(text, variables);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
        }

    }
}
