using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commandline
{
    /// <summary>
    /// A page with all the basic functionality to input and run registered <see cref="Command"/> instances, or assign functions to specific keys
    /// </summary>
    public class CommandLine : ConsoleDialog
    {

        #region Command editor

        /// <summary>
        /// The command currently in the input and edited by the user
        /// </summary>
        protected StringBuilder CurrentCommand = new StringBuilder(30);
        /// <summary>
        /// A string that will be displayed in front of the currently edited command, but itself can't be edited.
        /// </summary>
        public string CommandPrefix { get; protected set; } = ">";
        /// <summary>
        /// Tracks which previous command was grabbed from <see cref="CommandHistory"/> for navigating the history with the arrow keys.
        /// </summary>
        protected int HistoryIndex = 0;
        /// <summary>
        /// Contains all previously entered commands.
        /// </summary>
        public List<string> CommandHistory = new List<string>();
        /// <summary>
        /// Marks that <see cref="CurrentCommand"/> should be reset.
        /// </summary>
        protected bool EndOfCommand;

        #endregion

        #region Command execution

        /// <summary>
        /// The set of commands this instance can run, accessible by their ID
        /// </summary>
        protected Dictionary<string, Command> CommandSet = new Dictionary<string, Command>();
        /// <summary>
        /// The set of actions this instance will perform if the associated <see cref="ConsoleKeyInfo"/> is read.
        /// </summary>
        protected Dictionary<ConsoleKeyInfo, Action> FunctionKeys = new Dictionary<ConsoleKeyInfo, Action>();
        /// <summary>
        /// Marks that this instance will exit <see cref="RunKeyLoop"/> after the current command is done executing.
        /// </summary>
        protected bool EndingLoop;

        #endregion

        public override int Show()
        {
            RunKeyLoop();
            return ResultValue;
        }

        protected void RunKeyLoop()
        {
            while (!EndingLoop)
            {
                Console.Write(CommandPrefix);
                CurrentCommand.Clear();
                while (true)
                {
                    Action keyAction;
                    var keyPress = Console.ReadKey(true);

                    if (FunctionKeys.TryGetValue(keyPress, out keyAction))
                    {
                        keyAction.Invoke();
                        if (EndOfCommand)
                        {
                            EndOfCommand = false;
                            break;
                        }
                    }
                    else
                    {
                        if (keyPress.KeyChar != '\0')
                        {
                            if (Console.CursorLeft == CommandPrefix.Length + CurrentCommand.Length)
                            {
                                Console.Write(keyPress.KeyChar);
                                CurrentCommand.Append(keyPress.KeyChar);
                            }
                            else
                            {
                                var pos = Console.CursorLeft;
                                CurrentCommand.Insert(pos - CommandPrefix.Length, keyPress.KeyChar);
                                Console.Write(CurrentCommand.ToString().Substring(pos - CommandPrefix.Length));
                                Console.CursorLeft = pos + 1;
                            }
                        }

                    }
                }
            }
        }

        protected override void Init()
        {
            RegisterBaseKeys();

            AddCommand(new Command("help", Help, Help_AC));

            AddCommand(new Command("exit", (s) => { EndingLoop = true; }) {
                Description = "Ends the current process." });

            //AddCommand(new Command("debugkey", (s) =>
            //{
            //    var key = Console.ReadKey();
            //    Console.WriteLine();
            //    Console.WriteLine(key.KeyChar);
            //    Console.WriteLine((int)key.KeyChar);
            //    Console.WriteLine(key.Key);
            //}));

            AddCommand(new Command("cat", (s) => 
            {
                string[] cmdParams;
                Util.TrySplitCommand(s, out cmdParams);
                for (int i = 1; i < cmdParams.Length; i++)
                {
                    Console.WriteLine(cmdParams[i]);
                }
            }));
        }

        protected void RegisterBaseKeys()
        {
            FunctionKeys[new ConsoleKeyInfo('\t', ConsoleKey.Tab, false, false, false)] = AutoComplete;
            FunctionKeys[new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)] = ProcessCommand;

            FunctionKeys[new ConsoleKeyInfo('\r', ConsoleKey.Enter, true, false, false)] = () => {
                CurrentCommand.Append('\n');
                Console.WriteLine();
            };

            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false)] = () => {
                if (Console.CursorLeft < CurrentCommand.Length + CommandPrefix.Length)
                    Console.CursorLeft++;
            };

            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false)] = () => {
                if (Console.CursorLeft > CommandPrefix.Length)
                    Console.CursorLeft--;
            };

            FunctionKeys[new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false)] = () => {
                if (Console.CursorLeft > CommandPrefix.Length)
                {
                    var pos = Console.CursorLeft;
                    Console.CursorLeft--;
                    Console.Write(new string(' ', CommandPrefix.Length + CurrentCommand.Length - Console.CursorLeft));
                    CurrentCommand.Remove(pos - CommandPrefix.Length - 1, 1);
                    Console.CursorLeft = pos - 1;
                    Console.Write(CurrentCommand.ToString().Substring(Console.CursorLeft - CommandPrefix.Length));
                    Console.CursorLeft = pos - 1;
                }
            };

            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.Delete, false, false, false)] = () => {
                if (Console.CursorLeft < CommandPrefix.Length + CurrentCommand.Length) {
                    var pos = Console.CursorLeft;
                    Console.Write(new string(' ', CommandPrefix.Length + CurrentCommand.Length - Console.CursorLeft));
                    CurrentCommand.Remove(pos - CommandPrefix.Length, 1);
                    Console.CursorLeft = pos;
                    Console.Write(CurrentCommand.ToString().Substring(Console.CursorLeft - CommandPrefix.Length));
                    Console.CursorLeft = pos;
                }
            };

            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false)] = () => {
                if (CommandHistory.Count != 0)
                {
                    Console.CursorLeft = CommandPrefix.Length;
                    Console.Write(new string(' ', CurrentCommand.Length));
                    Console.CursorLeft = CommandPrefix.Length;
                    HistoryIndex = HistoryIndex == 0 ? CommandHistory.Count - 1 : HistoryIndex - 1;
                    CurrentCommand.Clear();
                    CurrentCommand.Append(CommandHistory[HistoryIndex]);
                    Console.Write(CommandHistory[HistoryIndex]);
                }
            };

            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false)] = () => {
                if (CommandHistory.Count != 0)
                {
                    Console.CursorLeft = CommandPrefix.Length;
                    Console.Write(new string(' ', CurrentCommand.Length));
                    Console.CursorLeft = CommandPrefix.Length;
                    HistoryIndex = HistoryIndex == CommandHistory.Count - 1 ? 0 : HistoryIndex + 1;
                    CurrentCommand.Clear();
                    CurrentCommand.Append(CommandHistory[HistoryIndex]);
                    Console.Write(CommandHistory[HistoryIndex]);
                }
            };

        }

        protected void AutoComplete()
        {
            string complete = string.Empty;
            var command = CurrentCommand.ToString();
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
            CurrentCommand.Append(complete);
            Console.Write(complete);
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

        #region Exposing CommandSet

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

        #endregion

        protected CommandResult RunCommand(string cmd)
        {
            return CommandSet[cmd.ReadToCharOrEnd(' ')].Function.Invoke(cmd);
        }

        protected void ProcessCommand()
        {
            string cmd = CurrentCommand.ToString();
            CommandHistory.Add(cmd);
            Console.WriteLine();
            string cmdId = cmd.ReadToCharOrEnd(' ');
            if (CommandSet.ContainsKey(cmdId))
            {
                var result = RunCommand(CurrentCommand.ToString());
                if (result.Resultcode != 0)
                {
                    Console.WriteLine(result.Message);
                }
            } else
            {
                Console.WriteLine("\"{0}\" is not recognized as a command. Try \"help\"", cmdId);
            }
            EndOfCommand = true;
        }
        /// <summary>
        /// Returns the index of the character that the current cursor position will edit in <see cref="CurrentCommand"/>
        /// </summary>
        protected int GetCurrEditPos()
        {
            return 0;
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

        protected CommandResult Exit(string cmd)
        {
            EndingLoop = true;
            return CommandResult.Success;
        }
    }
}
