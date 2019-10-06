using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commandline
{

    public class CommandPage : ConsolePage
    {
        protected StringBuilder CurrentCommand = new StringBuilder(30);
        protected Dictionary<string, Command> CommandSet = new Dictionary<string, Command>();
        protected Dictionary<ConsoleKeyInfo, Action> FunctionKeys = new Dictionary<ConsoleKeyInfo, Action>();
        protected bool EndingLoop;
        protected bool EndOfCommand;
        protected int historyIndex = 0;

        public List<string> CommandHistory = new List<string>();

        public string CommandPrefix { get; protected set; } = ">";
        public bool AllowMultiline { get; set; } = false;

        public override int StartPage()
        {
            StartKeyLoop();
            return ResultValue;
        }

        protected void StartKeyLoop()
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
                            if (!AllowMultiline && CommandPrefix.Length + CurrentCommand.Length == Console.BufferWidth) continue;
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

            AddCommand(new Command("help", Help, Help_AC) {
                Description = "Prints the list of available commands, or provides help with the secified one.",
                Help = "help [command]" });

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
            if (GetType() == typeof(CommandPage))
            {

            }
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
                    historyIndex = historyIndex == 0 ? CommandHistory.Count - 1 : historyIndex - 1;
                    CurrentCommand.Clear();
                    CurrentCommand.Append(CommandHistory[historyIndex]);
                    Console.Write(CommandHistory[historyIndex]);
                }
            };

            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false)] = () => {
                if (CommandHistory.Count != 0)
                {
                    Console.CursorLeft = CommandPrefix.Length;
                    Console.Write(new string(' ', CurrentCommand.Length));
                    Console.CursorLeft = CommandPrefix.Length;
                    historyIndex = historyIndex == CommandHistory.Count - 1 ? 0 : historyIndex + 1;
                    CurrentCommand.Clear();
                    CurrentCommand.Append(CommandHistory[historyIndex]);
                    Console.Write(CommandHistory[historyIndex]);
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
