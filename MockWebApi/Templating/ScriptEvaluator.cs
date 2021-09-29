using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Threading.Tasks;

namespace MockWebApi.Templating
{
    public class ScriptEvaluator
    {

        private readonly ScriptOptions _options;

        //private readonly object _globals;

        private Script _script;
        private ScriptState _scriptState;

        public ScriptEvaluator(string scriptInitCode = null)
        {
            _options = ScriptOptions.Default
                .AddImports("System")
                .AddImports("System.Text")
                .AddImports("System.Text.Json")
                .AddImports("System.Linq")
                .AddImports("System.Xml")
                .AddImports("System.Collections")
                .AddImports("System.Collections.Generic");

            _script = CSharpScript.Create(scriptInitCode ?? "", _options);
        }

        public async Task<object> RunLineOfCodeAsync(string lineOfCode)
        {
            _scriptState = await RunLineOfCodeFromAsync(lineOfCode, _scriptState);

            object result = _scriptState.ReturnValue;
            // variables that were defined in the script can be accessed
            // throught the _scriptState.Variables[0].Name .Type and .Value

            return result;
        }

        private async Task<ScriptState> RunLineOfCodeFromAsync(string lineOfCode, ScriptState scriptState)
        {
            Script newScript = _script.ContinueWith(lineOfCode);
            ScriptState newScriptState = null;

            try
            {
                if (scriptState == null)
                {
                    newScriptState = await newScript.RunAsync(catchException: ExceptionHandler);
                }
                else
                {
                    newScriptState = await newScript.RunFromAsync(scriptState, catchException: ExceptionHandler);
                }
            }
            catch (Exception ex)
            {
                newScriptState = scriptState;
                Console.WriteLine($"An error occured during execution of the script: { ex.Message }");
            }

            _script = newScript;
            return newScriptState;
        }

        private bool ExceptionHandler(Exception ex)
        {

            return true;
        }

    }
}
