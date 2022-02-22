using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Threading.Tasks;

namespace MockWebApi.Templating
{
    public class ScriptEvaluator
    {

        public ScriptEvaluator(
            string? scriptInitCode = null,
            string? lineInitCode = null)
        {
            _options = ScriptOptions.Default
                .AddImports("System")
                .AddImports("System.Text")
                .AddImportReference("System.Text.Json")
                .AddImportReference("System.Linq")
                .AddImportReference("System.Xml")
                .AddImportReference("System.Collections")
                .AddImportReference("System.Collections.Generic");

            _script = CSharpScript.Create(scriptInitCode ?? "", _options);
            _lineInitCode = lineInitCode ?? "";
        }

        private readonly ScriptOptions _options;

        //private readonly object _globals;

        private Script _script;
        private ScriptState? _scriptState;

        private readonly string _lineInitCode;

        private const string CONSOLE_REDIRECT_VARIABLE_NAME = "___ConsoleOutputRedirectionTextWriter___";
        private readonly string _redirectConsoleOutput =
            $"System.IO.TextWriter {CONSOLE_REDIRECT_VARIABLE_NAME} = new System.IO.StringWriter();\n" +
            $"Console.SetOut({CONSOLE_REDIRECT_VARIABLE_NAME});\n";

        public async Task<object> RunLineOfCodeAsync(string lineOfCode)
        {
            _scriptState = await RunLineOfCodeFromAsync(lineOfCode, _scriptState);

            object? result = _scriptState?.ReturnValue;
            // variables that were defined in the script can be accessed
            // throught the _scriptState.Variables[0].Name .Type and .Value

            ScriptVariable? scriptVariable = _scriptState?.GetVariable(CONSOLE_REDIRECT_VARIABLE_NAME);
            string consoleOutput = scriptVariable?.Value.ToString()!;

            return result ?? consoleOutput;
        }

        private async Task<ScriptState?> RunLineOfCodeFromAsync(string lineOfCode, ScriptState? scriptState)
        {
            Script scriptStep = _script
                .ContinueWith(_redirectConsoleOutput)
                .ContinueWith(_lineInitCode)
                .ContinueWith(lineOfCode);

            ScriptState? newScriptState = null;

            try
            {
                newScriptState = await RunAsync(scriptStep, scriptState);
            }
            catch (Exception ex)
            {
                newScriptState = scriptState;
                Console.WriteLine($"An error occured during execution of the script: { ex.Message }");
            }

            _script = scriptStep;
            return newScriptState;
        }

        private async Task<ScriptState> RunAsync(Script script, ScriptState? scriptState)
        {
            ScriptState? newScriptState = null;

            if (scriptState == null)
            {
                newScriptState = await script.RunAsync(catchException: ExceptionHandler);
            }
            else
            {
                newScriptState = await script.RunFromAsync(scriptState, catchException: ExceptionHandler);
            }

            return newScriptState;
        }

        private bool ExceptionHandler(Exception ex)
        {

            return true;
        }

    }
}
