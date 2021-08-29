using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Templating
{
    public interface ITemplateParser
    {

        Template Parse(string templateText);

    }
}
