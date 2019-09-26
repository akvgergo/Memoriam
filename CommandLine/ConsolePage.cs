using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memoriam
{
    public abstract class ConsolePage
    {
        protected readonly string InitArgs;
        public string ResultMessage { protected set; get; }
        protected int ResultValue = 0;

        public ConsolePage(string initArgs)
        {
            InitArgs = initArgs;
            Init();
        }

        public ConsolePage()
        {
            Init();
        }

        public abstract int StartPage();

        protected abstract void Init();
        
    }
}
