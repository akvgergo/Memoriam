using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;


namespace Memoriam
{
    class Program
    {
        static void Main(string[] args)
        {

            CommandPage startPage = new CommandPage();
            startPage.AddCommand(new Command("zip", (s) =>
            {
                string[] cmdParams;
                if (!Util.TrySplitCommand(s, out cmdParams))
                {
                    return new CommandResult(-1, "Unable to parse command.");
                }
                if (File.Exists(cmdParams[1]))
                {
                    return new CommandResult(-1, "file already exists");
                }

                using (ZipFile zip = ZipFile.Create(cmdParams[1]))
                {
                    zip.BeginUpdate();
                    zip.Password = "password";
                    File.WriteAllText("text.txt", "Lorem ipsum\ndolor sit amet");
                    ZipEntryFactory fac = new ZipEntryFactory();
                    var entry = fac.MakeFileEntry("text.txt");
                    entry.AESKeySize = 256;
                    zip.Add(entry);
                    zip.CommitUpdate();
                }

                return CommandResult.Success;
            })
            {
                Description = "Creates a zip file",
                Help = "zip <filename>"
            });
            startPage.StartPage();
        }

    }
}
