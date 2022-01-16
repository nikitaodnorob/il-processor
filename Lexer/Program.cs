using System;
using System.IO;

namespace Lexer
{
    class Program
    {
        static void Main(string[] args)
        {
            string testIlCode = File.ReadAllText(@"D:\Windows\Desktop\MasterDiploma\MasterDiploma\01_ulearn_rectangles\author1\my_release.il");
            Lexer.GetLexemes(testIlCode);
        }
    }
}