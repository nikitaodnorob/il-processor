using System.Collections.Generic;

namespace ILLexer
{
    public partial class Lexer
    {
        private static readonly List<string> Keywords = new List<string>
        {
            "public",
            "protected",
            "private",
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
            "void",
            "valuetype",
            "specialname",
            "rtspecialname",
            "native",
            "assembly",
            "only",
            "nested",
            "serializable",
            "endfinally",
            "finally",
            "literal",
            "virtual",
            "final",
            "implements",
            "unsigned",
            "newslot",
        };
    }
}