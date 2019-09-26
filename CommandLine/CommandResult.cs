using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memoriam
{
    public class CommandResult
    {
        public int resultcode { get; private set; }
        public string message { get; private set; }

        public CommandResult(int code, string msg = "")
        {
            resultcode = code;
            message = msg;
        }

        public static CommandResult Success {
            get {
                return new CommandResult(0);
            }
        }

    }
}
