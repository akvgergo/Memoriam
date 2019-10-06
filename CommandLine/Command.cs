using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commandline
{
    public class Command
    {
        public string Id { get; private set; }
        public string Description { get; set; } = "<No description available>";
        public string Help { get; set; } = "<No help available>";
        public Func<string, CommandResult> Function { get; private set; }
        public Func<string, string> AutoComplete { get; private set; }

        public Command(string id, Func<string, CommandResult> function, Func<string, string> autoComplete = null)
        {
            Id = id;
            Function = function;
            AutoComplete = autoComplete == null ? (s) => { return string.Empty; } : autoComplete;
        }

        public Command(string id, Action<string> action, Func<string, string> autoComplete = null)
        {
            Id = id;
            Function = (s) => { action.Invoke(s); return CommandResult.Success; };
            AutoComplete = autoComplete == null ? (s) => { return string.Empty; } : autoComplete;
        }
    }
}
