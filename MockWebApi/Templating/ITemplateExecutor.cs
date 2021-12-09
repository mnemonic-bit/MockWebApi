using System.Collections.Generic;
using System.Threading.Tasks;

namespace MockWebApi.Templating
{
    public interface ITemplateExecutor
    {

        public Task<string> Execute(string templateText, IDictionary<string, string> vairables);

    }
}
