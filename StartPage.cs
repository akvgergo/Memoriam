using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memoriam
{
    public class StartPage : CommandPage
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
