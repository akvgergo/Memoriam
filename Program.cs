﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Ionic.Zip;
using Commandline;
using System.Threading;


namespace Memoriam
{
    class Program
    {   
        static void Main(string[] args)
        {
            KeyReaderDialog dialog = new KeyReaderDialog();
            dialog.Show();
        }
    }
}
