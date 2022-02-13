using System.Collections.Generic;

namespace Lexer
{
    public partial class Lexer
    {
        private static readonly List<string> Keywords = new List<string>
        {
            "public",
            "abstract",
            "sealed",
            "auto",
            "ansi",
            "beforefieldinit",
            "extends",
            "class",
            "hidebysig",
            "static",
            "cil",
            "managed",
            "instance",
            "init",
        };
    }
}