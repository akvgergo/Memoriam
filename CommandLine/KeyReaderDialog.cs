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
        /// Marks that <see cref="FormatField(bool)"/> should be called when appropriate.
        /// </summary>
        protected bool ShouldFormat { get; set; }

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

        /// <summary>
        /// Ads the specified character to <see cref="FieldRows"/>
        /// </summary>
        /// <param name="character">The character to append or insert.</param>
        protected void AddText(char character)
        {
            var row = FieldRows[CurrentRow];
            if (row.Length >= 80)
            {
                
            }
            row.Append(character);
            MoveCursor();
        }

        protected void ShiftChars(int amount)
        {
            //sanity
            if (amount == 0) return;

            if (amount > 0)
            {
                string word;
                for (int rowIndex = CurrentRow; rowIndex < FieldRows.Count; rowIndex++)
                {
                    
                }
            }
            
        }

        protected virtual void RunKeyLoop()
        {
            while (!EndingLoop)
            {
                Console.Write(Prefix);
                FieldRows.Select(sb => sb.Clear());
                CurrentChar = 0;
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
                        } else
                        {
                            ShouldFormat = true;
                        }
                    }
                    else
                    {
                        if (keyPress.KeyChar != '\0')
                        {
                            AddText(keyPress.KeyChar);
                        }
                    }
                    if (ShouldFormat)
                        FormatField();
                    Console.CursorVisible = true;
                }
            }
        }

        /// <summary>
        /// Should be called before we remove characters from a row.
        /// <para>This just removes all the current visible characters in the row buffer. Does not actually change the underlying text.</para>
        /// </summary>
        /// <param name="row">The specific row to clear. -1 Clears the entire field.</param>
        protected void ClearBuffer(int row = -1)
        {
            //sanity
            if (row > FieldRows.Count - 1) throw new ArgumentOutOfRangeException("row");

            var curPos = new { X = Console.CursorLeft, Y = Console.CursorTop };
            string clear = new string('\0', Console.BufferWidth);
            if (row < 0)
            {
                Console.SetCursorPosition(Prefix.Length, FieldBeginPos);
                for (int i = 0; i < FieldRows.Count; i++)
                {
                    Console.Write(clear);
                }
            } 
            else
            {
                Console.SetCursorPosition(0, FieldBeginPos + row);
                Console.Write(clear);
            }
            Console.SetCursorPosition(curPos.X, curPos.Y);
        }

        /// <summary>
        /// Moves the cursor a specified amount, moving between rows as necessary and keeping <see cref="CurrentChar"/> in sync.
        /// Can receive and safely handle absurd values, should never overflow in any direction.
        /// </summary>
        /// <param name="amount">How many characters and indexes to move. Negative to move backwards.</param>
        protected void MoveCursor(int amount = 1)
        {
            //sanity
            if (amount == 0) return;
            //underflow
            if (CurrentChar + amount < 0)
            {
                CurrentChar = 0;
                Console.SetCursorPosition(Prefix.Length, FieldBeginPos);
            }
            //overflow
            else if (CurrentChar + amount > FieldText.Length)
            {
                CurrentChar = FieldText.Length;
                FormatField(false);
            }
            //standard
            else
            {
                //TODO: -gergő this NEEDS to be optimized, for at least 1 and -1 values
                CurrentChar += amount;
                FormatField();
            }
        }

        //UNDONE: -gergő needs work
        /// <summary>
        /// Attempts to format the text in the current field in a sensible way
        /// </summary>
        /// <param name="keepCurPos"></param>
        private void FormatField(bool keepCurPos = true)
        {
            ClearBuffer();
            
            Console.SetCursorPosition(Prefix.Length, FieldBeginPos);

            FieldHeight = 1;
            int curPosX = Console.CursorTop == FieldBeginPos ? Prefix.Length : 0;
            var curPosY = FieldBeginPos;
            var searchEditIndex = keepCurPos;
            StringBuilder row = new StringBuilder(Console.BufferWidth);
            StringBuilder word = new StringBuilder();
            int width = Console.BufferWidth - Prefix.Length;
            int i;

            //split up the text in a way that it neatly fits the console, splitting into words then rows as it fits
            //also searching for the cursor position that corresponds to EditIndex
            for (i = 0; i < FieldText.Length; i++)
            {
                bool isWhiteSpace = char.IsWhiteSpace(FieldText[i]);
                word.Append(FieldText[i]);

                //the word itself is bigger than a row should be, nothing we can format
                if (word.Length >= width)
                {
                    Console.Write(row);
                    row.Clear();
                    Console.Write(word);
                    word.Clear();
                    if (searchEditIndex) curPosY++;
                    FieldHeight++;
                    width = Console.BufferWidth;
                    continue;
                }

                //if the current word added to the row would be bigger than the console horizontal buffer, we start a new row instead
                if (row.Length + word.Length > width)
                {
                    Console.WriteLine(row);
                    row.Clear();
                    if (searchEditIndex) curPosY++;
                    FieldHeight++;
                    width = Console.BufferWidth;
                }

                //each whitespace creates a new word
                if (isWhiteSpace)
                {
                    if (FieldText[i] == '\n') //also start a new row if the user input has line breaks
                    {
                        row.Append(word);
                        word.Clear();
                        Console.Write(row);
                        row.Clear();
                        if (searchEditIndex) curPosY++;
                        FieldHeight++;
                        width = Console.BufferWidth;
                    }

                    row.Append(word);
                    word.Clear();
                }

                //the current cursor position is the EditIndex, remember that
                if (CurrentChar - 1 == i)
                {
                    curPosX = Console.CursorTop == FieldBeginPos ? Prefix.Length + row.Length + word.Length : row.Length + word.Length;
                    searchEditIndex = false;
                }
            }

            row.Append(word);
            Console.Write(row);

            if (CurrentChar == i)
                curPosX = Console.CursorTop == FieldBeginPos ? Prefix.Length + row.Length : row.Length;
            

            if (keepCurPos)
            {
                if (curPosX >= Console.BufferWidth)
                {
                    curPosY += curPosX / Console.BufferWidth;
                    curPosX %= Console.BufferWidth;
                }
                Console.SetCursorPosition(curPosX, curPosY);
            }

            ShouldFormat = false;
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
                FieldText.Insert(CurrentChar, '\n');
                MoveCursor();
            };

            FunctionKeys[new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false)] = () => {
                if (FieldText.Length == 0 || CurrentChar == 0) return;
                ClearBuffer();
                FieldText.Remove(CurrentChar - 1, 1);
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
