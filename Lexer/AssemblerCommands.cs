using System.Collections.Generic;

namespace Lexer
{
    public partial class Lexer
    {
        private static readonly List<string> AssemblerCommands = new List<string>
        {
            "newobj",
            "stsfld",
            "callvirt",
            "call",
            "ret",
            "ldfld",
            "ldc.i4.m1",
            "ldloca.s",
            "stloc.s",
            "bgt.s",
            "cgt",
            "ceq"
        };
        
        private static readonly List<string> ParametrizedAssemblerCommands = new List<string>
        {
            "ldarg",
            "ldc.i4",
            "ldloc",
        };
    }
}