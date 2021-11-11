using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
