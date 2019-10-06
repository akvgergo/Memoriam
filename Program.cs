using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Ionic.Zip;
using Commandline;

namespace Memoriam
{
    class Program
    {   
        [CommandInfo("","")]
        static void Main(string[] args)
        {
            
            foreach (var item in System.Reflection.MethodBase.GetCurrentMethod().GetCustomAttributes(false))
            {
                var help = item as CommandInfoAttribute;
                if (help == null) continue;
                help.
            }
        }

    }
}
