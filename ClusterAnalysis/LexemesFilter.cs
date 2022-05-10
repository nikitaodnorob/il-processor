using ILLexer;

namespace ClusterAnalysis;

public static class LexemesFilter
{
    public static List<Lexeme> Filter(List<Lexeme> code)
    {
        code = FilterPunctuation(code);
        code = FilterUselessKeywords(code);
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
