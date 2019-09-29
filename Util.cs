using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memoriam
{
    public static class Util
    {

        public static string ReadToCharOrEnd(this string s, char c)
        {
            int index = 0;
            StringBuilder builder = new StringBuilder();
            while (index < s.Length && s[index] != c) builder.Append(s[index++], 1);

            if (index == s.Length) return s;
            else return builder.ToString();
        }

        public static string ReadToCharOrEnd(this StringBuilder sb, char c)
        {
            int index = 0;
            StringBuilder builder = new StringBuilder();
            while (index < sb.Length && sb[index] != c) builder.Append(sb[index++], 1);

            return builder.ToString();
        }

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
