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

            string scriptInitCode = GenerateInitScript(variables);
            ScriptEvaluator scriptEvaluator = new ScriptEvaluator(scriptInitCode);

            var calcualtedFragments = await Task.WhenAll(template
                .Fragments
                .Select(fragment => EvaluateFragment(scriptEvaluator, fragment)));

            string result = string.Concat(calcualtedFragments);

            return result;
        }

        private async Task<string> EvaluateFragment(ScriptEvaluator scriptEvaluator, Fragment fragment)
        {
            dynamic realFragment = fragment;
            return await EvaluateFragment(scriptEvaluator, realFragment);
        }

        private async Task<string> EvaluateFragment(ScriptEvaluator scriptEvaluator, ScriptFragment fragment)
        {
            object result = await scriptEvaluator.RunLineOfCodeAsync(fragment.ScriptText);
            return result?.ToString() ?? "";
        }

        private Task<String> EvaluateFragment(ScriptEvaluator scriptEvaluator, StringFragment fragment)
        {
            return Task.FromResult(fragment.Text);
        }

        private string GenerateInitScript(IDictionary<string, string> variables)
        {
            string initializeVariables = string.Join("\n", variables.Select(g => $"string {g.Key} = \"{g.Value}\";"));

            return initializeVariables;
        }

    }
}
