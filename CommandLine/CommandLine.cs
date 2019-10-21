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
        /// The command currently in the input and edited by the user.
        /// </summary>
        protected StringBuilder CurrentCommand = new StringBuilder(30);
        /// <summary>
        /// The the console row <see cref="CurrentCommand"/> begins on.
        /// </summary>
        protected int CommandRow = 0;
        /// <summary>
        /// The index of the character that the current cursor position will edit in <see cref="CurrentCommand"/>.
        /// </summary>
        protected int EditIndex = 0;
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

        protected override void Init()
        {
            RegisterBaseKeys();
            AddCommand(new Command("help", Help, Help_AC));
            AddCommand(new Command("exit", Exit));
        }

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
                EditIndex = 0;
                CommandRow = Console.CursorTop;
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
                            CurrentCommand.Insert(EditIndex, keyPress.KeyChar);
                            MoveCursor();
                        }
                    }
                    FormatCurrentCommand();
                }
            }
        }

        private void FormatCurrentCommand(bool keepCurPos = true)
        {
            var curPos = new { X = Console.CursorLeft, Y = Console.CursorTop };

            Console.SetCursorPosition(CommandPrefix.Length, CommandRow);
            Console.Write(new string(CurrentCommand.ToString().Select(c => c == '\r' || c == '\n' ? c : ' ').ToArray()));

            Console.SetCursorPosition(CommandPrefix.Length, CommandRow);
            Console.Write(CurrentCommand.ToString());

            if (keepCurPos)
                Console.SetCursorPosition(curPos.X, curPos.Y);
        }

        /// <summary>
        /// Should be called before we remove characters from <see cref="CurrentCommand"/>.
        /// <para>This just removes all the current visible characters in the buffer</para>
        /// </summary>
        protected void ClearRow()
        {
            var curPos = new { X = Console.CursorLeft, Y = Console.CursorTop };
            Console.SetCursorPosition(CommandPrefix.Length, CommandRow);
            Console.Write(new string(CurrentCommand.ToString().Select(c => c == '\r' || c == '\n' ? c : ' ').ToArray()));
            Console.SetCursorPosition(curPos.X, curPos.Y);
        }

        protected void MoveCursor(int amount = 1)
        {
            if (EditIndex + amount < 0)
            {
                EditIndex = 0;
                Console.SetCursorPosition(CommandPrefix.Length, CommandRow);
                return;
            }
            else if (EditIndex + amount > CurrentCommand.Length)
            {
                EditIndex = CurrentCommand.Length;
                FormatCurrentCommand(false);
                return;
            }
            else
            {
                if (Console.CursorLeft + amount < Console.BufferWidth && Console.CursorLeft + amount > 0)
                {
                    Console.CursorLeft += amount;
                    EditIndex += amount;
                    return;
                }
                else
                {
                    Console.CursorTop += amount % Console.BufferWidth;
                    Console.CursorLeft += amount % (Console.BufferWidth - 1);
                    EditIndex += amount;
                }
            }
        }

        #region Exposing command and key registry

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

        #region Default commands and key behaviour

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

        protected void RegisterBaseKeys()
        {
            FunctionKeys[new ConsoleKeyInfo('\t', ConsoleKey.Tab, false, false, false)] = AutoComplete;
            FunctionKeys[new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)] = ProcessCommand;

            FunctionKeys[new ConsoleKeyInfo('\r', ConsoleKey.Enter, true, false, false)] = () => {
                CurrentCommand.Insert(EditIndex, '\r');
                MoveCursor();
            };

            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false)] = () => {
                MoveCursor();
            };

            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false)] = () => {
                MoveCursor(-1);
            };

            FunctionKeys[new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false)] = () => {
                if (CurrentCommand.Length == 0 || EditIndex == 0) return;
                ClearRow();
                CurrentCommand.Remove(EditIndex - 1, 1);
                MoveCursor(-1);
            };

            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.Delete, false, false, false)] = () => {
                if (CurrentCommand.Length == 0 || EditIndex == CurrentCommand.Length) return;
                ClearRow();
                CurrentCommand.Remove(EditIndex, 1);
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

        #endregion

    }
}
