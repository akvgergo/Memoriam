using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commandline
{
    public abstract class ConsoleDialog
    {
        /// <summary>
        /// The arguments this instance was created with, if any.
        /// </summary>
        protected string InitArgs;
        /// <summary>
        /// The object parameter this instance was created with, if any.
        /// </summary>
        protected virtual object[] InitParams { get; set; }
        /// <summary>
        /// A message of the result that should be displayed to the user, if there is any.
        /// </summary>
        public string ResultMessage { protected set; get; }
        /// <summary>
        /// Indicates whether this instance ran successfully or encountered any errors.
        /// </summary>
        public virtual int ResultValue { get; protected set; }

        /// <summary>
        /// Creates and initializes a new instance with the specified argument string.
        /// </summary>
        /// <param name="initArgs">The initialization arguments for this instance.</param>
        public ConsoleDialog(string initArgs)
        {
            InitArgs = initArgs;
            Init();
        }

        /// <summary>
        /// Creates and initializes a new instace with an optional argument string, and additional <see cref="object"/> parameters
        /// </summary>
        /// <param name="initArgs">The initialization arguments for this instance.</param>
        /// <param name="param">Objects to initialize this instance with.</param>
        public ConsoleDialog(string initArgs = "", params object[] param)
        {
            InitArgs = initArgs;
            InitParams = param;
            Init();
        }

        /// <summary>
        /// Creates and initializes a new instace with no parameters.
        /// </summary>
        public ConsoleDialog()
        {
            Init();
        }

        /// <summary>
        /// Displays this instance and starts any long-running operations, including keyloops or anything that will take over Console write operations.
        /// </summary>
        /// <returns></returns>
        public abstract int Show();

        /// <summary>
        /// Initializes this instance. Any operation too large for the constructor should go here.
        /// </summary>
        protected abstract void Init();
        
    }
}
