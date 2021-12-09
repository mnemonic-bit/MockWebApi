using Microsoft.CodeAnalysis.Scripting;

namespace MockWebApi.Templating
{
    public static class ScriptOptionsExtensions
    {

        public static ScriptOptions AddImportReference(this ScriptOptions scriptOptions, params string[] importReferences)
        {
            return scriptOptions
                .AddImports(importReferences)
                .AddReferences(importReferences);
        }

    }
}
