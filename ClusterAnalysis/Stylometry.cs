using ILLexer;

namespace ClusterAnalysis;

public static class Stylometry
{
    public static StylometryCodeData CalcStylometryData(List<Lexeme> lexemes)
    {
        if (Settings.IsLexemesFiltering) lexemes = LexemesFilter.Filter(lexemes);

        var uniqLexemesSet = new HashSet<string>();
        lexemes.ForEach(lexeme => uniqLexemesSet.Add(lexeme.LexemeText));

        double lexicalDiversity = 1d * uniqLexemesSet.Count / lexemes.Count;

        var methodsLength = new List<int>();
        bool isMethodDeclaration = false;
        bool isMethod = false;

        var methodsLocalVarsCnt = new List<int>();
        bool isMethodLocalVars = false;

        var methodsMaxStack = new List<int>();
        bool isMethodMaxStack = false;

        foreach (var lexeme in lexemes)
        {
            if (!isMethod && lexeme.Kind == LexemeKind.Directive && lexeme.LexemeText == ".method")
                isMethodDeclaration = true;

            if (isMethodDeclaration && lexeme.Kind == LexemeKind.LeftFigureBracket)
            {
                isMethodDeclaration = false;
                isMethod = true;
                methodsLength.Add(0);
                methodsLocalVarsCnt.Add(0);
                methodsMaxStack.Add(0);
            }

            if (isMethod && lexeme.Kind == LexemeKind.AssemblerCommand && lexeme.LexemeText != "nop")
                methodsLength[^1]++;

            if (isMethod && lexeme.Kind == LexemeKind.Directive && lexeme.LexemeText == ".locals")
                isMethodLocalVars = true;

            if (isMethodLocalVars && lexeme.Kind == LexemeKind.Directive && lexeme.LexemeText.StartsWith("["))
                methodsLocalVarsCnt[^1]++;

            if (isMethodLocalVars && lexeme.Kind == LexemeKind.RightRoundBracket) isMethodLocalVars = false;

            if (isMethod && lexeme.Kind == LexemeKind.Directive && lexeme.LexemeText == ".maxstack")
                isMethodMaxStack = true;

            if (isMethodMaxStack && lexeme.Kind == LexemeKind.NumberLiteral)
            {
                methodsMaxStack[^1] = int.Parse(lexeme.LexemeText);
                isMethodMaxStack = false;
            }

            if (isMethod && lexeme.Kind == LexemeKind.RightFigureBracket)
                isMethod = false;
        }

        return new StylometryCodeData
        {
            LexicalDiversity = lexicalDiversity,
            MethodsAvgLength = methodsLength.Count > 0 ? methodsLength.Average() : 0,
            MethodsLocalVarsAvgCnt = methodsLocalVarsCnt.Count > 0 ? methodsLocalVarsCnt.Average() : 0,
            MethodsMaxStackAvg = methodsMaxStack.Count > 0 ? methodsMaxStack.Average() : 0,
        };
    }

    public static double CalcDistance(
        StylometryCodeData dataA,
        StylometryCodeData dataB,
        double codesCosDistance,
        double stringLiteralsDistance,
        double numberLiteralsDistance
    )
    {
        if (codesCosDistance > 0.5) return 1;
        
        var vecA = new[]
        {
            dataA.LexicalDiversity * 10,
            dataA.MethodsAvgLength,
            dataA.MethodsMaxStackAvg,
            dataA.MethodsLocalVarsAvgCnt,
        };

        var vecB = new[]
        {
            dataB.LexicalDiversity * 10,
            dataB.MethodsAvgLength,
            dataB.MethodsMaxStackAvg,
            dataB.MethodsLocalVarsAvgCnt,
        };

        double distance = Vector.CosDistance(vecA, vecB);

        if (codesCosDistance < 0.3)
        {
            double correctedSimilarity =
                (1d - distance) *
                ((1 - stringLiteralsDistance) / 2 + 0.5) *
                ((1 - numberLiteralsDistance) / 2 + 0.5);
            return 1 - correctedSimilarity;
        }

        return distance;
    }
}

/*
N1 (really one cluster) = 131
N2 (wrong one cluster) = 220
N3 (wrong different clusters) = 557
Accuracy = 0.24651248164464024
*/