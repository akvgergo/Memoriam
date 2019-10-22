using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commandline
{
    /// <summary>
    /// A dialog that acts as a standard text editor, and can run functions associated with specific keys.
    /// </summary>
    public class KeyReaderDialog : ConsoleDialog
    {
        /// <summary>
        /// The command currently in the input and edited by the user.
        /// </summary>
        protected StringBuilder CurrentText = new StringBuilder(30);
        /// <summary>
        /// The the console row <see cref="CurrentText"/> begins on.
        /// </summary>
        protected int CurrentField = 0;
        /// <summary>
        /// The index of the character that the current cursor position will edit in <see cref="CurrentText"/>.
        /// </summary>
        protected int EditIndex = 0;
        /// <summary>
        /// A string that will be displayed in front of the currently edited command, but itself can't be edited.
        /// </summary>
        public string Prefix { get; protected set; } = ">";
        /// <summary>
        /// Marks that this instance will exit <see cref="RunKeyLoop"/> after the current operation is done executing.
        /// </summary>
        protected bool EndingLoop;
        /// <summary>
        /// Marks that <see cref="CurrentText"/> should be reset.
        /// </summary>
        protected bool EndOfInput;
        /// <summary>
        /// Marks that <see cref="FormatBuffer(bool)"/> should be called, when appropriate.
        /// </summary>
        protected bool ShouldFormat;

        /// <summary>
        /// The set of actions this instance will perform if the associated <see cref="ConsoleKeyInfo"/> is read.
        /// </summary>
        protected Dictionary<ConsoleKeyInfo, Action> FunctionKeys = new Dictionary<ConsoleKeyInfo, Action>();

        /// <summary>
        /// In this instance, restores most Console functionality that is lost by running <see cref="Console.ReadKey"/> in a loop,
        /// and ads some extra functionality, mostly reminiscent of text editors.
        /// </summary>
        protected override void Init()
        {
            RegisterBaseKeys();
        }

        public override int Show()
        {
            RunKeyLoop();
            return ResultValue;
        }

        protected virtual void RunKeyLoop()
        {
            while (!EndingLoop)
            {
                Console.Write(Prefix);
                CurrentText.Clear();
                EditIndex = 0;
                CurrentField = Console.CursorTop;
                while (true)
                {
                    Action keyAction;
                    var keyPress = Console.ReadKey(true);

                    if (FunctionKeys.TryGetValue(keyPress, out keyAction))
                    {
                        keyAction.Invoke();
                        if (EndOfInput)
                        {
                            EndOfInput = false;
                            break;
                        }
                    }
                    else
                    {
                        if (keyPress.KeyChar != '\0')
                        {
                            CurrentText.Insert(EditIndex, keyPress.KeyChar);
                            MoveCursor();
                        }
                    }
                    FormatBuffer();
                }
            }
        }

        /// <summary>
        /// Should be called before we remove characters from <see cref="CurrentText"/>.
        /// <para>This just removes all the current visible characters in the buffer.</para>
        /// </summary>
        protected void ClearRow()
        {
            Console.CursorVisible = false;
            var curPos = new { X = Console.CursorLeft, Y = Console.CursorTop };
            Console.SetCursorPosition(Prefix.Length, CurrentField);
            Console.Write(new string(CurrentText.ToString().Select(c => c == '\r' || c == '\n' ? c : ' ').ToArray()));
            Console.SetCursorPosition(curPos.X, curPos.Y);
            Console.CursorVisible = true;
        }

        /// <summary>
        /// Moves the cursor a specified amount, moving between rows as necessary and keeping <see cref="EditIndex"/> in sync.
        /// Can receive and safely handle absurd values, should never overflow in any direction.
        /// </summary>
        /// <param name="amount">How many characters and indexes to move. Negative to move backwards.</param>
        protected void MoveCursor(int amount = 1)
        {
            Console.CursorVisible = false;

            //sanity
            if (amount == 0) return;
            //underflow
            if (EditIndex + amount < 0)
            {
                EditIndex = 0;
                Console.SetCursorPosition(Prefix.Length, CurrentField);
                return;
            }
            //overflow
            else if (EditIndex + amount > CurrentText.Length)
            {
                EditIndex = CurrentText.Length;
                FormatBuffer(false);
                return;
            }
            //standard
            else
            {
                //TODO: this NEEDS to be optimized, for at least 1 and -1 values
                string cmd = CurrentText.ToString();
                Console.SetCursorPosition(Prefix.Length, CurrentField);
                Console.Write(cmd.Substring(0, EditIndex + amount));
                FormatBuffer();
                EditIndex += amount;
            }

            Console.CursorVisible = true;
        }
        //TODO: needs work
        private void FormatBuffer(bool keepCurPos = true)
        {
            ClearRow();
            var curPos = new { X = 0, Y = 0 };
            Console.SetCursorPosition(Prefix.Length, CurrentField);

            StringBuilder row = new StringBuilder(Console.BufferWidth);
            StringBuilder word = new StringBuilder();
            int width = Console.BufferWidth - Prefix.Length - 2;
            for (int i = 0; i < CurrentText.Length; i++)
            {
                word.Append(CurrentText[i]);
                if (!char.IsWhiteSpace(CurrentText[i]))
                {
                    if (row.Length + word.Length > width)
                    {
                        Console.WriteLine(row);
                        row.Clear();
                        width = Console.BufferWidth - 2;
                    }
                    row.Append(word);
                    word.Clear();
                }
            }
            Console.WriteLine(row.Append(word));

            if (keepCurPos)
                Console.SetCursorPosition(curPos.X, curPos.Y);
        }

        protected virtual void RegisterBaseKeys()
        {
            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false)] = () => MoveCursor();
            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, true)] = () => {
                MoveCursor(CurrentText.ToString().Substring(EditIndex)
                    .TakeWhile(c => !char.IsWhiteSpace(c)).Count() + 1);
            };
            
            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false)] = () => MoveCursor(-1);
            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, true)] = () => {
                MoveCursor(-CurrentText.ToString().Reverse().Skip(CurrentText.Length - EditIndex + 1)
                    .TakeWhile(c => !char.IsWhiteSpace(c)).Count() - 1);
            };

            FunctionKeys[new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)] = () => {
                CurrentText.Insert(EditIndex, '\n');
                MoveCursor();
            };

            FunctionKeys[new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false)] = () => {
                if (CurrentText.Length == 0 || EditIndex == 0) return;
                ClearRow();
                CurrentText.Remove(EditIndex - 1, 1);
                MoveCursor(-1);
            };

            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.Delete, false, false, false)] = () => {
                if (CurrentText.Length == 0 || EditIndex == CurrentText.Length) return;
                ClearRow();
                CurrentText.Remove(EditIndex, 1);
            };
        }
    }
}
