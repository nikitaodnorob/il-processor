using System.Collections.Generic;

namespace Lexer
{
    public partial class Lexer
    {
        private static List<string> AssemblerCommands = new List<string>
        {
            "newobj",
            "stsfld",
            "call",
            "ret",
            "ldarg.0"
        };
    }
}