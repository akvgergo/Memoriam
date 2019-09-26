using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace Memoriam
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandPage startPage = new CommandPage();
            startPage.AddCommand(new Command("ziptext", (s) =>
            {
                var cmdParams = s.Split(' ');
                if (cmdParams.Length < 2 || File.Exists(cmdParams[1] + ".txt") || File.Exists(cmdParams[1] + ".zip"))
                    return new CommandResult(-1, "IO error: files already exist");
                File.Create(cmdParams[1] + ".txt").Dispose();
                var proc = Process.Start(cmdParams[1] + ".txt");
                Console.WriteLine("Don't forget to save!");
                proc.WaitForExit();
                using (FileStream stream = new FileStream(cmdParams[1] + ".zip", FileMode.Create))
                {
                    ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create);
                    archive.CreateEntryFromFile(cmdParams[1] + ".txt", cmdParams[1] + ".txt", CompressionLevel.Optimal);
                    archive.Dispose();
                }
                File.Delete(cmdParams[1] + ".txt");
                Process.Start(cmdParams[1] + ".zip");
                return new CommandResult(1, "Success!");
            })
            {
                Description = "Creates a txt, opens it for the user, then compresses the saved file",
                Help = "ziptext <filename>"
            });
            startPage.StartPage();
        }

    }
}
