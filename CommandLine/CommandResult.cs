using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commandline
{
    /// <summary>
    /// Default object returned by commands. This isn't the return value of the underlying method called,
    /// but rather a result indicating if the command was succesful or not.
    /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// Indicates whether the underlying method ran succesfully. 0 is a silent success with no necessary message to display.
        /// Positive values denote a success with details attached, while negative values indicate an error.
        /// </summary>
        public int Resultcode { get; private set; }

        /// <summary>
        /// Message displayed if the <see cref="Resultcode"/> is non-zero.
        /// </summary>
        public string Message { get; private set; }
        static CommandResult _success = new CommandResult(0);

        /// <summary>
        /// Creates a new <see cref="CommandResult"/> instance.
        /// </summary>
        /// <param name="code">The <see cref="Resultcode"/> of this instance. Positive/negative denote success/error respectively. 0 is silent.</param>
        /// <param name="msg">The attached message with error or success details.</param>
        public CommandResult(int code, string msg = "")
        {
            Resultcode = code;
            Message = msg;
        }

        /// <summary>
        /// Returns a <c>CommandResult</c> instance with a <see cref="Resultcode"/> of 0.
        /// </summary>
        public static CommandResult Success {
            get {
                return _success;
            }
        }

    }
}
