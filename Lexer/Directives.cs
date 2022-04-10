using System.Collections.Generic;

namespace ILLexer
{
    public partial class Lexer
    {
        private static readonly List<string> Directives = new List<string>
        {
            ".property",
            ".maxstack",
            ".class",
            ".method",
            ".locals",
            ".custom",
            ".field",
            ".param",
            ".cctor",
            ".ctor",
            ".try",
            ".get",
            ".set",
            "[out]",
            "[0]",
            "[1]",
            "[2]",
            "[3]",
            "[4]",
            "[5]",
            "[6]",
            "[7]",
            "[8]",
            "[9]",
        };
    }
}