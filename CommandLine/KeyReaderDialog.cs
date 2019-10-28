using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commandline
{
    /// <summary>
    /// A dialog that acts as a standard text editor, and can run functions associated with specific keys. Can be easily derived from for other dialogs dealing with
    /// text based user input.
    /// </summary>
    public class KeyReaderDialog : ConsoleDialog
    {

        const int NEW_FIELD_HEIGHT = 4;

        /// <summary>
        /// The entire field of text currently in the input and edited by the user.
        /// <para>Only for getting the entire text for final operations. For editing the field contents, see <see cref="FieldRows"/>.</para>
        /// </summary>
        protected string FieldText {
            get {
                StringBuilder builder = new StringBuilder(FieldRows.Select(sb => sb.Length).Sum());
                for (int i = 0; i < FieldRows.Count; i++) builder.Append(FieldRows[i]);
                return builder.ToString();
            }
        }
        /// <summary>
        /// All the rows and their contents that are not empty. Editing should be done here.
        /// </summary>
        /// <remarks>
        /// In earlier versions, this was a single stringbuilder. Went with this approach instead because:
        /// -easier to handle rows separately
        /// -easier breakdown of logic
        /// -easier syncing of the recorded text and what is actually shown in the console, let alone writing and cursor positions
        /// -only minor to no performance decrease
        /// </remarks>
        protected List<StringBuilder> FieldRows = new List<StringBuilder>(NEW_FIELD_HEIGHT); //default would be 16, 4 seems plenty
        /// <summary>
        /// The console buffer row <see cref="FieldText"/> begins on.
        /// </summary>
        protected int FieldBeginPos = 0;
        /// <summary>
        /// The index of the character that the current cursor position will edit in <see cref="FieldRows"/>.
        /// </summary>
        protected int CurrentChar = 0;
        /// <summary>
        /// The current row in <see cref="FieldRows"/> being edited.
        /// </summary>
        protected int CurrentRow = 0;
        /// <summary>
        /// A string that will be displayed in front of the currently edited field, but itself can't be edited.
        /// </summary>
        public virtual string Prefix { get; protected set; } = ">";
        /// <summary>
        /// Marks that this instance will exit <see cref="RunKeyLoop"/> after the current operation is done executing.
        /// </summary>
        protected bool EndingLoop { get; set; }
        /// <summary>
        /// Marks that <see cref="FieldText"/> should be reset.
        /// </summary>
        protected bool EndOfInput { get; set; }
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
            for (int i = 0; i < NEW_FIELD_HEIGHT; i++) FieldRows.Add(new StringBuilder(Console.BufferWidth));
            RegisterBaseKeys(); 
        }

        public override int Show()
        {
            RunKeyLoop();
            return ResultValue;
        }

        /// <summary>
        /// Adds the specified character to <see cref="FieldRows"/>.
        /// </summary>
        /// <param name="character">The character to append or insert.</param>
        protected void AddText(char character)
        {
            var row = FieldRows[CurrentRow];
            if (row.Length >= Console.BufferWidth - (CurrentRow == 0 ? Prefix.Length : 0))
            {
                ShiftInsert(character.ToString());
                return;
            }
            row.Insert(CurrentChar, character);
            SyncBuffer(CurrentRow);
            MoveCursor();
        }

        /// <summary>
        /// Adds the specified string to <see cref="FieldRows"/>.
        /// </summary>
        /// <param name="text">The string to append or insert</param>
        protected void AddText(string text)
        {
            var row = FieldRows[CurrentRow];
            if (row.Length + text.Length >= Console.BufferWidth - (CurrentRow == 0 ? Prefix.Length : 0))
            {
                ShiftInsert(text);
                return;
            }
            row.Insert(CurrentChar, text);
            SyncBuffer(CurrentRow);
            MoveCursor(text.Length);
        }

        protected void ClearField()
        {
            ClearBuffer();
            FieldRows.Select(sb => sb.Clear());
            Console.SetCursorPosition(0, FieldBeginPos);
            Console.Write(Prefix);
        }

        protected void SyncBuffer(int row = -1) {

            //sanity
            if (row >= FieldRows.Count) throw new ArgumentOutOfRangeException("row");

            //for placing the cursor back later
            var curPos = new { X = Console.CursorLeft, Y = Console.CursorTop };

            //sync everything
            if (row < 0)
            {
                ClearBuffer();
                Console.SetCursorPosition(0, FieldBeginPos);
                Console.Write(Prefix);
                foreach (var item in FieldRows)
                {
                    Console.Write(row);
                }
                Console.SetCursorPosition(curPos.X, curPos.Y);
                return;
            }

            //sync the specifc row
            ClearBuffer(row);
            Console.SetCursorPosition(0, row + FieldBeginPos);
            if (CurrentRow == 0) Console.Write(Prefix);
            Console.Write(FieldRows[row]);

            Console.SetCursorPosition(curPos.X, curPos.Y);
        }

        protected void ShiftInsert(string text)
        {
            //TODO do-while? 
            StringBuilder carry = new StringBuilder();
            var row = FieldRows[CurrentRow];
            row.Insert(CurrentChar, text);
            int rowIndex = CurrentRow + 1;
            var rowLengthGoal = Console.BufferWidth - (CurrentRow == 0 ? Prefix.Length : 0);

            //TODO -gergő could be optimized I'm sure
            var carryStr = new string(row.ToString().Reverse().TakeWhile((c, i) => char.IsWhiteSpace(c) && row.Length - i <= rowLengthGoal).Reverse().ToArray());
            carry.Append(carryStr);
            row.Remove(row.Length - carryStr.Length, carryStr.Length);

            //moving the excess down the rows
            while (carry.Length != 0)
            {
                //make sure the field is big enough
                if (rowIndex == FieldRows.Count) FieldRows.Add(new StringBuilder(Console.BufferWidth));

                //we can just insert the row if the length is just right, or if it would be a new line anyway
                if (carry.Length == Console.BufferWidth || carry[carry.Length -1] == '\n')
                {
                    FieldRows.Insert(rowIndex, carry);
                    break;
                }
                
                //the length is smaller than the buffer, we're done
                if (FieldRows[rowIndex].Length + carry.Length < Console.BufferWidth)
                {
                    FieldRows[rowIndex].Insert(0, carry);
                    break;
                }

                //reitarete on this. worst case would be going through all the rows
                FieldRows[rowIndex].Insert(0, carry);
                carry.Clear();

                carryStr = new string(FieldRows[rowIndex].ToString().Reverse().TakeWhile((c, i) => char.IsWhiteSpace(c) && FieldRows[rowIndex].Length - i <= Console.BufferWidth).Reverse().ToArray());
                carry.Append(carryStr);
                FieldRows[rowIndex].Remove(FieldRows[rowIndex].Length - carryStr.Length, carryStr.Length);
                rowIndex++;
            }

            //redraw the buffer
            for (int i = CurrentRow; i <= rowIndex; i++) SyncBuffer(i);

        }

        protected virtual void RunKeyLoop()
        {

            do
            {
                Console.Write(Prefix);
                FieldRows.Select(sb => sb.Clear());
                CurrentChar = 0;
                CurrentRow = 0;
                FieldBeginPos = Console.CursorTop;
                while (true)
                {
                    Action keyAction;
                    var keyPress = Console.ReadKey(true);
                    Console.CursorVisible = false;

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
                            AddText(keyPress.KeyChar);
                        }
                    }
                    Console.CursorVisible = true;
                }
            } while (!EndingLoop);
        }

        /// <summary>
        /// Should be called before we remove characters from a row.
        /// <para>This just removes all the current visible characters in the row buffer. Does not actually change the underlying text.</para>
        /// </summary>
        /// <param name="row">The specific row to clear. -1 Clears the entire field.</param>
        protected void ClearBuffer(int row = -1)
        {
            //sanity
            if (row >= FieldRows.Count) throw new ArgumentOutOfRangeException("row");

            //not changing the cursor position
            var curPos = new { X = Console.CursorLeft, Y = Console.CursorTop };

            string clear = new string('\0', Console.BufferWidth);
            //clear all
            if (row < 0)
            {
                Console.SetCursorPosition(0, FieldBeginPos);
                for (int i = 0; i < FieldRows.Count; i++)
                {
                    Console.Write(clear);
                }
            } 
            else //clear specified
            {
                Console.SetCursorPosition(0, FieldBeginPos + row);
                Console.Write(clear);
            }

            Console.SetCursorPosition(curPos.X, curPos.Y);
        }

        /// <summary>
        /// Moves the cursor a specified amount, moving between rows as necessary and keeping
        /// <see cref="CurrentChar"/> and <see cref="CurrentRow"/> in sync.
        /// </summary>
        /// <param name="amount">How many characters and indexes to move. Negative to move backwards.</param>
        protected void MoveCursor(int amount = 1)
        {
            //sanity
            if (amount == 0) return;

            int charIndex = CurrentChar + amount;

            if (charIndex >= 0 && charIndex < Console.BufferWidth)
            {
                CurrentChar += amount;
            }
            else
            {
                if (charIndex > 0)
                {
                    amount -= CurrentChar;
                    while (amount >= FieldRows[CurrentRow].Length)
                    {
                        amount -= FieldRows[CurrentRow++].Length;
                    }
                    CurrentChar += amount;
                }
                else
                {
                    amount += CurrentChar;
                    while (amount <= FieldRows[CurrentRow].Length)
                    {
                        amount += FieldRows[CurrentRow--].Length;
                    }
                    CurrentChar -= amount;
                }
            }

            Console.SetCursorPosition((CurrentRow == 0 ? Prefix.Length : 0) + CurrentChar, FieldBeginPos + CurrentRow);
        }

        protected virtual void RegisterBaseKeys()
        {
            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false)] = () => MoveCursor();
            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, true)] = () => {
                MoveCursor(FieldText.ToString().Substring(CurrentChar)
                    .TakeWhile(c => !char.IsWhiteSpace(c)).Count() + 1);
            };
            
            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false)] = () => MoveCursor(-1);
            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, true)] = () => {
                MoveCursor(-FieldText.ToString().Reverse().Skip(FieldText.Length - CurrentChar + 1)
                    .TakeWhile(c => !char.IsWhiteSpace(c)).Count() - 1);
            };

            FunctionKeys[new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)] = () => {
                AddText('\n');
                MoveCursor();
            };

            FunctionKeys[new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false)] = () => {
                if (FieldText.Length == 0 || CurrentChar == 0) return;
                FieldRows[CurrentRow].Remove(CurrentChar - 1, 1);
                SyncBuffer(CurrentRow);
                MoveCursor(-1);
            };

            FunctionKeys[new ConsoleKeyInfo('\0', ConsoleKey.Delete, false, false, false)] = () => {
                if (FieldText.Length == 0 || CurrentChar == FieldText.Length) return;
                ClearBuffer();
                FieldText.Remove(CurrentChar, 1);
            };
        }
    }
}
