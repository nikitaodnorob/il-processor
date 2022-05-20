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
        code = code.Where(lexeme => lexeme.LexemeText != "," && lexeme.LexemeText != "").ToList();

        if (Settings.DistanceMetric != DistanceMetric.Stylometry)
            code = code.Where(lexeme => !punctuationChars.Contains(lexeme.LexemeText)).ToList();

        return code.ToList();
    }

    private static readonly List<string> uselessKeywords = new()
    {
        "sealed", "auto", "ansi", "beforefieldinit", "hidebysig", "managed", "valuetype", "final",
        "specialname", "rtspecialname", "native", "assembly", "only", "nested", "serializable", "newslot"
    };

    private static readonly List<string> punctuationChars = new() { "{", "}", "[", "]", "(", ")" };

    private static List<Lexeme> FilterUselessKeywords(List<Lexeme> code)
    {
        code = code.Where(lexeme => !uselessKeywords.Contains(lexeme.LexemeText)).ToList();

        if (Settings.DistanceMetric != DistanceMetric.Stylometry)
            code = code.Where(lexeme => lexeme.LexemeText != ".maxstack" && lexeme.LexemeText != ".locals").ToList();

        return code;
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
                i < code.Count - 3 &&
                code[i].Kind == LexemeKind.Label &&
                code[i + 1].Kind == LexemeKind.AssemblerCommand && code[i + 1].LexemeText == "br.s" &&
                code[i + 2].Kind == LexemeKind.Label &&
                code[i + 3].Kind == LexemeKind.Label &&
                code[i + 2].LexemeText == code[i + 3].LexemeText
            )
            {
                i += 2;
                continue;
            }

            if (code[i].Kind == LexemeKind.AssemblerCommand && code[i].LexemeText == "nop")
                continue;

            code2.Add(code[i]);
        }
        
        /* 11_ulearn_antiplagiat/author2/my_debug.il:346
            IL_0154: stloc.s V_9
            IL_0156: ldloc.s V_9
            
            or
            
            IL_0039: stloc.3
            IL_003a: ldloc.3
         */

        var code3 = new List<Lexeme>();
        for (int i = 0; i < code2.Count; i++)
        {
            if (
                i < code2.Count - 5 &&
                code2[i].Kind == LexemeKind.Label &&
                code2[i + 1].Kind == LexemeKind.AssemblerCommand && code2[i + 1].LexemeText == "stloc.s" &&
                code2[i + 2].Kind == LexemeKind.Entity &&
                code2[i + 3].Kind == LexemeKind.Label &&
                code2[i + 4].Kind == LexemeKind.AssemblerCommand && code2[i + 4].LexemeText == "ldloc.s" &&
                code2[i + 5].Kind == LexemeKind.Entity &&
                code2[i + 2].LexemeText == code2[i + 5].LexemeText
            )
            {
                i += 5;
                continue;
            }
            
            if (
                i < code2.Count - 3 &&
                code2[i].Kind == LexemeKind.Label &&
                code2[i + 1].Kind == LexemeKind.AssemblerCommand && code2[i + 1].LexemeText.StartsWith("stloc.") &&
                code2[i + 2].Kind == LexemeKind.Label &&
                code2[i + 3].Kind == LexemeKind.AssemblerCommand && code2[i + 3].LexemeText.StartsWith("ldloc.") &&
                code2[i + 1].LexemeText.Split('.')[1] == code2[i + 3].LexemeText.Split('.')[1]
            )
            {
                i += 3;
                continue;
            }
            
            code3.Add(code2[i]);
        }

        var code4 = new List<Lexeme>();

        bool isLocalVars = false;
        int? methodEndInd = null;
        
        for (int i = 0; i < code3.Count; i++)
        {
            if (code3[i].Kind == LexemeKind.Directive && code3[i].LexemeText == ".locals")
            {
                isLocalVars = true;
                methodEndInd = code3.FindIndex(i + 1, lexeme => lexeme.Kind == LexemeKind.RightFigureBracket);
            }

            if (isLocalVars && code3[i].Kind == LexemeKind.RightRoundBracket)
                isLocalVars = false;
            
            if (
                isLocalVars &&
                i < code3.Count - 3 &&
                code3[i].Kind == LexemeKind.Directive && code3[i].LexemeText.StartsWith('[') &&
                code3[i + 1].Kind == LexemeKind.Entity &&
                code3[i + 2].Kind == LexemeKind.Entity
            )
            {
                int varNumber = int.Parse(code3[i].LexemeText.Trim('[', ']'));
                int localVarUsageInd = code3.FindIndex(i + 3, lexeme => (
                    lexeme.Kind == LexemeKind.AssemblerCommand && lexeme.LexemeText == $"ldloc.{varNumber}" ||
                    lexeme.Kind == LexemeKind.AssemblerCommand && lexeme.LexemeText == $"stloc.{varNumber}" ||
                    lexeme.Kind == LexemeKind.Entity && lexeme.LexemeText == code3[i + 2].LexemeText
                ));

                if (localVarUsageInd == -1 || localVarUsageInd > methodEndInd)
                {
                    // Console.ForegroundColor = ConsoleColor.Yellow;
                    // Console.WriteLine($"unused local var {code3[i + 2].LexemeText}");
                    // Console.ResetColor();
                    i += 2;
                    continue;
                }
            }
            code4.Add(code3[i]);
        }

        return code4;
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
