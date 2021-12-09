using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Templating
{
    public class TemplateExecutor : ITemplateExecutor
    {

        private readonly ITemplateParser _templateParser;

        public TemplateExecutor(ITemplateParser templateParser)
        {
            _templateParser = templateParser;
        }

        public async Task<string> Execute(string templateText, IDictionary<string, string> variables)
        {
            Template template = _templateParser.Parse(templateText);

            string[] parameters = await Evaluate(variables, template.Parameters);

            string result = string.Format(template.FormatString, parameters);

            return result;
        }

        private async Task<string[]> Evaluate(IDictionary<string, string> vairables, string[] templateExpressions)
        {
            string scriptInitCode = GenerateInitScript(vairables);

            ScriptEvaluator scriptEvaluator = new ScriptEvaluator(scriptInitCode);

            IList<string> evaluatedTemplateExpressions = new List<string>();
            foreach (string templateExpression in templateExpressions)
            {
                object result = await scriptEvaluator.RunLineOfCodeAsync(templateExpression);
                evaluatedTemplateExpressions.Add(result?.ToString() ?? "");
            }

            return evaluatedTemplateExpressions.ToArray();
        }

        private string GenerateInitScript(IDictionary<string, string> variables)
        {
            string initializeVariables = string.Join("\n", variables.Select(g => $"string {g.Key} = \"{g.Value}\";"));

            return initializeVariables;
        }

    }
}
