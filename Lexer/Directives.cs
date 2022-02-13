using System.Collections.Generic;

namespace Lexer
{
    public partial class Lexer
    {
        private static readonly List<string> Directives = new List<string>
        {
            ".class",
            ".method",
            ".field",
            ".maxstack",
            ".locals"
        };
    }
}