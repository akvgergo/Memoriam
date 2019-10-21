using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commandline
{
    /// <summary>
    /// Represents a command that a <see cref="CommandLine"/> instance can use.
    /// </summary>
    public class Command
    {
        /// <summary>
        /// The unique identifier and first word of the command.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Short description of the command's purpose. Displayed by the "help" command by default.
        /// </summary>
        public string Description { get; set; } = "<No description available>";

        /// <summary>
        /// <para>Describes how the command should be used.</para>
        /// <para>Vague convention is [necessary parameter], "user decided string", &lt;optional parameter&gt;, -additional, {parsed text}. These can be stacked. Examples:</para>
        /// <para>create_txt ["filename"] &lt;"contents"&gt; -overwrite</para><para>create_obj [{json}]</para>
        /// <para>Validating, escaping, parsing, etc. is typically on the underlying function, the method will always receive the raw command unless you use a custom page.</para>
        /// </summary>
        public string Help { get; set; } = "<No help available>";

        /// <summary>
        /// The underlying method of the command. This method will be called whenever the command's first word is the <see cref="Id"/>.
        /// <para>Validating and escaping should be done in the method if you are using a standard <see cref="CommandLine"/>.</para>
        /// </summary>
        public Func<string, CommandResult> Function { get; private set; }

        /// <summary>
        /// The default <see cref="CommandLine"/> will run this method if the currently edited command already begins with this command's <see cref="Id"/>.
        /// <para>This method should return a string that will complete the current command parameter, or <see cref="string.Empty"/>
        /// in cases of invalid command syntax or no appropriate suggestion.</para>
        /// </summary>
        public Func<string, string> AutoComplete { get; private set; }

        /// <summary>
        /// Creates a new <see cref="Command"/> instance with the specified <see cref="Id"/>,
        /// a delegate for the Command's method, and optionally an autocomplete function.
        /// <para>The <see cref="Description"/> and <see cref="Help"/> properties will get populated
        /// if the <paramref name="function"/> is marked with <see cref="CommandInfoAttribute"/></para>
        /// </summary>
        /// <param name="id">The unique identifier and first word of the command.</param>
        /// <param name="function">The <see cref="Function"/> of this command.</param>
        /// <param name="autoComplete">The autocomplete method associated with this <see cref="Command"/>.</param>
        public Command(string id, Func<string, CommandResult> function, Func<string, string> autoComplete = null)
        {
            TryPopulateInfo(function);
            Id = id;
            Function = function;
            AutoComplete = autoComplete == null ? (s) => { return string.Empty; } : autoComplete;
        }

        /// <summary>
        /// Creates a new <see cref="Command"/> instance with the specified <see cref="Id"/>,
        /// a delegate for the Command's method, and optionally an autocomplete function.
        /// <para>The <see cref="Description"/> and <see cref="Help"/> properties will get populated
        /// if the <paramref name="function"/> is marked with <see cref="CommandInfoAttribute"/></para>
        /// </summary>
        /// <param name="id">The unique identifier and first word of the command.</param>
        /// <param name="action">The <see cref="Function"/> of this command, cast into a <see cref="Func{T, TResult}"/>, that always returns <see cref="CommandResult.Success"/>.</param>
        /// <param name="autoComplete">The autocomplete method associated with this <see cref="Command"/>.</param>
        public Command(string id, Action<string> action, Func<string, string> autoComplete = null)
        {
            TryPopulateInfo(action);
            Id = id;
            Function = (s) => { action.Invoke(s); return CommandResult.Success; };
            AutoComplete = autoComplete == null ? (s) => { return string.Empty; } : autoComplete;
        }

        /// <summary>
        /// Populates the <see cref="Help"/> and <see cref="Description"/> fields, if the <see cref="Function"/>
        /// field of this instance has an associated <see cref="CommandInfoAttribute"/>.
        /// </summary>
        private void TryPopulateInfo(Delegate d)
        {
            var helpAttr = d.Method.GetCustomAttributes(false)
                .Where(o => o.GetType() == typeof(CommandInfoAttribute))
                .Select(o => (CommandInfoAttribute)o).FirstOrDefault();

            if (helpAttr == null) return;
            Help = helpAttr.Help;
            Description = helpAttr.Description;
        }
    }
}
