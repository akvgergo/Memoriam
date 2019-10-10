using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commandline
{
    /// <summary>
    /// Contains utility methods for various purposes that were not worth making a separate class.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Reads the string until a specified character, or returns a copy if that character is not in the string.
        /// </summary>
        /// <param name="c">The character at which the copying should stop.</param>
        public static string ReadToCharOrEnd(this string s, char c)
        {
            int index = 0;
            StringBuilder builder = new StringBuilder();
            while (index < s.Length && s[index] != c) builder.Append(s[index++], 1);
            
            if (index == s.Length) return s;
            else return builder.ToString();
        }

        /// <summary>
        /// Reads the StringBuilder until a specified character, or returns a copy if that character is not in the StringBuilder.
        /// </summary>
        /// <param name="c">The character at which the copying should stop.</param>
        public static string ReadToCharOrEnd(this StringBuilder sb, char c)
        {
            int index = 0;
            StringBuilder builder = new StringBuilder();
            while (index < sb.Length && sb[index] != c) builder.Append(sb[index++], 1);

            return builder.ToString();
        }

        /// <summary>
        /// Tries to split the string into segments using the specified separator, but does not create a new segment if
        /// the separater is enclosed in the specified escape character.
        /// <para>Useful for splitting raw command strings.</para>
        /// </summary>
        /// <param name="cmd">The string to split.</param>
        /// <param name="cmdParams">The array to contain the segments.</param>
        /// <param name="separator">The character to split the string on.</param>
        /// <param name="separatorEsc">The character, which if encountered, will not split further until another <paramref name="separatorEsc"/> is encountered.</param>
        /// <returns>A <see cref="bool"/> that is <see cref="true"/> if no syntax errors were encountered.</returns>
        public static bool TrySplitCommand(string cmd, out string[] cmdParams, char separator = ' ', char separatorEsc = '"')
        {
            var index = 0;
            StringBuilder builder = new StringBuilder();
            var segments = new List<string>();
            while (index < cmd.Length)
            {
                if (cmd[index] == separatorEsc)
                {
                    while (++index < cmd.Length && cmd[index] != separatorEsc) builder.Append(cmd[index]);
                    if (index == cmd.Length)
                    {
                        cmdParams = null;
                        return false;
                    }
                    segments.Add(builder.ToString());
                    builder.Clear();
                    if (++index == cmd.Length)
                    {
                        cmdParams = segments.ToArray();
                        return true;
                    }
                }

                if (cmd[index] == separator)
                {
                    if (builder.Length != 0)
                    {
                        segments.Add(builder.ToString());
                        builder.Clear();
                    }
                    index++;
                    continue;
                }
                builder.Append(cmd[index++]);
            }

            segments.Add(builder.ToString());
            cmdParams = segments.ToArray();
            return true;
        }

    }
}
