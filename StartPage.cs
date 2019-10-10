using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commandline;

namespace Memoriam
{
    public class StartPage : CommandLine
    {

        
        protected override void Init()
        {
            base.Init();

            
        }
        

        CommandResult Open(string cmd)
        {
            return CommandResult.Success;
        }

        CommandResult Create(string cmd)
        {
            return CommandResult.Success;
        }

        CommandResult Unpack(string cmd)
        {
            return CommandResult.Success;
        }

    }
}
