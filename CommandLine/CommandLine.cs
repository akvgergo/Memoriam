using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commandline
{
    /// <summary>
    /// A page with all the basic functionality to input and run registered <see cref="Command"/> instances.
    /// </summary>
    public class CommandLine : KeyReaderDialog
    {

        /// <summary>
        /// Tracks which previous command was grabbed from <see cref="CommandHistory"/> for navigating the history with the arrow keys.
        /// </summary>
        protected int HistoryIndex = 0;
        /// <summary>
        /// Contains all previously entered commands.
        /// </summary>
        public List<string> CommandHistory = new List<string>();
        /// <summary>
        /// The set of commands this instance can run, accessible by their ID
        /// </summary>
        protected Dictionary<string, Command> CommandSet = new Dictionary<string, Command>();

        protected override void Init()
        {
            RegisterBaseKeys();

            AddCommand(new Command("help", Help, Help_AC));
            AddCommand(new Command("exit", Exit));

            FunctionKeys[new ConsoleKeyInfo('\t', ConsoleKey.Tab, false, false, false)] = AutoComplete;
            FunctionKeys[new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)] = ProcessCommand;

            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false)] = () => {
                if (CommandHistory.Count != 0)
                {
                    ClearRow();
                    HistoryIndex = HistoryIndex == 0 ? CommandHistory.Count - 1 : HistoryIndex - 1;
                    CurrentText.Clear();
                    CurrentText.Append(CommandHistory[HistoryIndex]);
                    Console.Write(CommandHistory[HistoryIndex]);
                }
            };

            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false)] = () => {
                if (CommandHistory.Count != 0)
                {
                    ClearRow();
                    HistoryIndex = HistoryIndex == CommandHistory.Count - 1 ? 0 : HistoryIndex + 1;
                    CurrentText.Clear();
                    CurrentText.Append(CommandHistory[HistoryIndex]);
                    Console.Write(CommandHistory[HistoryIndex]);
                }
            };

        }




        public void AddCommand(Command cmd) {
            CommandSet.Add(cmd.Id, cmd);
        }

        public void AddCommand(string id, Action<string> func)
        {
            AddCommand(id, (s) => { func.Invoke(s); });
        }

        public void AddCommand(string id, Func<string, CommandResult> func)
        {
            AddCommand(new Command(id, func));
        }

        protected CommandResult RunCommand(string cmd)
        {
            return CommandSet[cmd.ReadToCharOrEnd(' ')].Function.Invoke(cmd);
        }

        protected void ProcessCommand()
        {
            string cmd = CurrentText.ToString();
            CommandHistory.Add(cmd);
            Console.WriteLine();
            string cmdId = cmd.ReadToCharOrEnd(' ');
            if (CommandSet.ContainsKey(cmdId))
            {
                var result = RunCommand(CurrentText.ToString());
                if (result.Resultcode != 0)
                {
                    Console.WriteLine(result.Message);
                }
            } else
            {
                Console.WriteLine("\"{0}\" is not recognized as a command. Try \"help\"", cmdId);
            }
            EndOfInput = true;
        }

        protected void AutoComplete()
        {
            string complete = string.Empty;
            var command = CurrentText.ToString();
            string[] cmd;
            if (!Util.TrySplitCommand(command, out cmd)) return;
            
            if (cmd.Length > 1)
            {
                Command result;
                if (CommandSet.TryGetValue(cmd [0], out result))
                {
                    complete = result.AutoComplete(command);
                }
            } else
            {
                foreach (var id in CommandSet.Keys)
                {
                    if (id.StartsWith(command))
                    {
                        complete = id.Remove(0, command.Length);
                    }
                }
            }
            CurrentText.Append(complete);
            Console.Write(complete);
        }

        [CommandInfo("Prints the list of available commands, or provides help with the secified one.", "help [command]")]
        protected CommandResult Help(string cmd)
        {
            var cmdParams = cmd.Split(' ');
            if (cmdParams.Length > 1)
            {
                Command value;
                if (!CommandSet.TryGetValue(cmdParams[1], out value))
                    return new CommandResult(1, "\"" + cmdParams[1] + "\" is not recognized as a command");

                Console.WriteLine(value.Help);

                return CommandResult.Success;
            }

            Console.WriteLine("Available commands:\n");
            foreach (var command in CommandSet)
            {
                Console.WriteLine("{0} : {1}", command.Key, command.Value.Description);
            }

            return CommandResult.Success;
        }

        [CommandInfo("Ends the current process.", "exit")]
        protected CommandResult Exit(string cmd)
        {
            EndingLoop = true;
            return CommandResult.Success;
        }

        protected string Help_AC(string cmd)
        {
            var command = cmd.Split(' ');
            if (command.Length > 2) return string.Empty;

            foreach (var id in CommandSet.Keys)
            {
                if (id.StartsWith(command[1]))
                {
                    return id.Remove(0, command[1].Length);
                }
            }

            return string.Empty;
        }
    }
}
