using ILLexer;

namespace ClusterAnalysis;

public static class LexemesFilter
{
    public static List<Lexeme> Filter(List<Lexeme> code)
    {
        var lexemesWithoutPunctuation = code.Where(lexeme => lexeme.LexemeText is not "," or "");

        if (Settings.DistanceMetric != DistanceMetric.Stylometry)
            lexemesWithoutPunctuation = code.Where(
                lexeme => lexeme.LexemeText is not "{" or "}" or "[" or "]" or "(" or ")"
            );

        var lexemesWithoutUselessKeywords = lexemesWithoutPunctuation.Where(
            lexeme => lexeme.LexemeText is not "sealed" or "auto" or "ansi" or "beforefieldinit"
                or "hidebysig" or "managed" or "valuetype" or "specialname" or "rtspecialname" 
                or "native" or "assembly" or "only" or "nested" or "serializable" or "literal"
                or ".maxstack" or ".locals" or ".custom"
        );

        var lexemesWithoutLabels = lexemesWithoutUselessKeywords.Where(
            lexeme => lexeme.Kind != LexemeKind.Label ||
                      lexeme.Kind == LexemeKind.AssemblerCommand && lexeme.LexemeText == "nop"
        );

        var lexemesWithCorrectedSharpEntities = lexemesWithoutLabels.Select(lexeme =>
        {
            if (lexeme.Kind != LexemeKind.Entity) return lexeme;
            return new Lexeme
            {
                Kind = LexemeKind.Entity, LexemePosition = lexeme.LexemePosition,
                LexemeText = lexeme.LexemeText
                    .Replace("[mscorlib]", "[library]")
                    .Replace("[System.Runtime]", "[library]")
                    .Replace("[System.Runtime.Extensions]", "[library]")
                    .Replace("[System.Core]", "[library]")
                    .Replace("[System.Linq]", "[library]")
            };
        });
        
        // Console.WriteLine($"{lexemes.Count} -> {lexemesWithCorrectedSharpEntities.Count()}");
        
        return lexemesWithCorrectedSharpEntities.ToList();
    }
}
