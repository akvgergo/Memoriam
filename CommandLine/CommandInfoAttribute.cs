using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commandline
{
    /// <summary>
    /// Describes usage information for a command that will be registered on a page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class CommandInfoAttribute : Attribute
    {
        /// <summary>
        /// <para>Describes how the command should be used.</para>
        /// <para>Vague convention is [necessary parameter], "user decided string", &lt;optional parameter&gt;, -additional, {parsed text}. These can be stacked.</para>
        /// <para>eg. create_txt ["filename"] &lt;"contents"&gt; -overwrite; or create_obj [{json}];</para>
        /// <para>Validating, escaping, parsing, etc. is typically on the underlying function, the method will always receive the raw command unless you use a custom page.</para>
        /// </summary>
        public string Help { get; private set; }
        /// <summary>
        /// Describes the command's purpose.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Creates a new <typeparamref name="CommandInfoAttribute"/> for a method.
        /// </summary>
        /// <param name="description">The description of the command that'll be processed by this method.</param>
        /// <param name="help">The syntax this command should be used with.</param>
        public CommandInfoAttribute(string description, string help)
        {
            Description = description;
            Help = help;
        }

    }
}
