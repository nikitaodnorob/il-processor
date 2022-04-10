using System.Collections.Generic;

namespace ILLexer
{
    public partial class Lexer
    {
        private static readonly List<string> AssemblerCommands = new List<string>
        {
            "constrained.",
            "ldelem.ref",
            "stelem.ref",
            "stelem.i2",
            "ldc.i4.m1",
            "ldc.i4.s",
            "stind.i4",
            "ldind.r8",
            "conv.i4",
            "ldloca.s",
            "ldloc.s",
            "stloc.s",
            "ldarg.s",
            "ldarga.s",
            "brtrue.s",
            "bne.un.s",
            "brfalse.s",
            "bge.un.s",
            "blt.un.s",
            "stind.r8",
            "starg.s",
            "leave.s",
            "conv.r8",
            "ldc.r8",
            "cgt.un",
            "blt.s",
            "bgt.s",
            "ble.s",
            "beq.s",
            "bge.s",
            "br.s",
            "blt.s",
            "callvirt",
            "ldelema",
            "ldtoken",
            "ldsfld",
            "ldnull",
            "newobj",
            "newarr",
            "stsfld",
            "ldstr",
            "ldlen",
            "ldftn",
            "ldfld",
            "stfld",
            "push",
            "call",
            "ret",
            "pop",
            "dup",
            "blt",
            "clt",
            "cgt",
            "ceq",
            "add",
            "sub",
            "mul",
            "div",
            "rem",
            "neg",
            "box",
            "nop",
            "br"
        };
        
        private static readonly List<string> ParametrizedAssemblerCommands = new List<string>
        {
            "ldarg",
            "ldc.i4",
            "ldloc",
            "stloc",
        };
    }
}