using ILLexer;

namespace ClusterAnalysis;

public static class LexemesFilter
{
    public static List<Lexeme> Filter(List<Lexeme> code)
    {
        code = FilterPunctuation(code);
        code = FilterUselessKeywords(code);
        code = FilterUselessAsmCommands(code);
        code = FilterLabels(code);
        code = FilterLinesEnd(code);
        code = CorrectEntities(code);

        return code;
    }

    private static List<Lexeme> FilterPunctuation(List<Lexeme> code)
    {
        var lexemesWithoutPunctuation = code.Where(lexeme => lexeme.LexemeText is not "," or "");

        if (Settings.DistanceMetric != DistanceMetric.Stylometry)
            lexemesWithoutPunctuation = code.Where(
                lexeme => lexeme.LexemeText is not "{" or "}" or "[" or "]" or "(" or ")"
            );

        return lexemesWithoutPunctuation.ToList();
    }

    private static List<Lexeme> FilterUselessKeywords(List<Lexeme> code)
    {
        return code.Where(
            lexeme => lexeme.LexemeText is not "sealed" or "auto" or "ansi" or "beforefieldinit"
                or "hidebysig" or "managed" or "valuetype" or "specialname" or "rtspecialname" 
                or "native" or "assembly" or "only" or "nested" or "serializable" or "literal"
                or ".maxstack" or ".locals" or ".custom"
        ).ToList();
    }

    private static List<Lexeme> FilterUselessAsmCommands(List<Lexeme> code)
    {
        /* 11_ulearn_antiplagiat/author2/my_debug.il:362
            IL_0178: br.s         IL_017a
            IL_017a: ldloc.s      V_10
         */

        var code2 = new List<Lexeme>();

        for (int i = 0; i < code.Count; i++)
        {
            if (
                i < code.Count - 5 &&
                code[i].Kind == LexemeKind.LineEnd &&
                code[i + 1].Kind == LexemeKind.Label &&
                code[i + 2].Kind == LexemeKind.AssemblerCommand && code[i + 2].LexemeText == "br.s" &&
                code[i + 3].Kind == LexemeKind.Label &&
                code[i + 4].Kind == LexemeKind.LineEnd &&
                code[i + 5].Kind == LexemeKind.Label &&
                code[i + 3].LexemeText == code[i + 5].LexemeText
            )
            {
                i += 4;
                continue;
            }

            if (code[i].Kind == LexemeKind.AssemblerCommand && code[i].LexemeText == "nop")
                continue;

            code2.Add(code[i]);
        }
        
        /* 11_ulearn_antiplagiat/author2/my_debug.il:346
            IL_0154: stloc.s V_9
            IL_0156: ldloc.s V_9
         */

        var code3 = new List<Lexeme>();
        for (int i = 0; i < code2.Count; i++)
        {
            if (
                i < code2.Count - 7 &&
                code2[i].Kind == LexemeKind.LineEnd &&
                code2[i + 1].Kind == LexemeKind.Label &&
                code2[i + 2].Kind == LexemeKind.AssemblerCommand && code2[i + 2].LexemeText == "stloc.s" &&
                code2[i + 3].Kind == LexemeKind.Entity &&
                code2[i + 4].Kind == LexemeKind.LineEnd &&
                code2[i + 5].Kind == LexemeKind.Label &&
                code2[i + 6].Kind == LexemeKind.AssemblerCommand && code2[i + 6].LexemeText == "ldloc.s" &&
                code2[i + 7].Kind == LexemeKind.Entity &&
                code2[i + 3].LexemeText == code2[i + 7].LexemeText
            )
            {
                i += 8;
                continue;
            }
            
            code3.Add(code2[i]);
        }

        return code3;
    }
    

    private static List<Lexeme> FilterLabels(List<Lexeme> code)
    {
        return code.Where(lexeme => lexeme.Kind != LexemeKind.Label).ToList();
    }
    
    private static List<Lexeme> FilterLinesEnd(List<Lexeme> code)
    {
        return code.Where(lexeme => lexeme.Kind != LexemeKind.LineEnd).ToList();
    }

    private static List<Lexeme> CorrectEntities(List<Lexeme> code)
    {
        return code.Select(lexeme =>
        {
            if (lexeme.Kind != LexemeKind.Entity) return lexeme;
            return new Lexeme
            {
                Kind = LexemeKind.Entity,
                LexemePosition = lexeme.LexemePosition,
                LexemeText = lexeme.LexemeText
                    .Replace("[mscorlib]", "[library]")
                    .Replace("[System.Runtime]", "[library]")
                    .Replace("[System.Runtime.Extensions]", "[library]")
                    .Replace("[System.Core]", "[library]")
                    .Replace("[System.Linq]", "[library]")
            };
        }).ToList();
    }
}
