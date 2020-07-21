using System;
using System.IO;
using Vim.DotNetUtilities;
using Vim.HelloWorld.Plugin;

namespace Vim.TextConverter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (!File.Exists(args[0]))
                throw new Exception("First argument must be an existing VIM file");
            if (!Directory.Exists(args[1]))
                throw new Exception("Second argument must be an existing output folder");
            Util.CreateAndClearDirectory(args[1]);
            ExportToText.Export(args[0], args[1]);
        }
    }
}
