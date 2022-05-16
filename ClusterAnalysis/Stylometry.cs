using ILLexer;

namespace ClusterAnalysis;

public static class Stylometry
{
    public static StylometryCodeData CalcStylometryData(List<Lexeme> lexemes)
    {
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

        int outCnt = 0;
        int literalsCnt = 0;

        foreach (var lexeme in lexemes)
        {
            if (lexeme.Kind == LexemeKind.Keyword && lexeme.LexemeText == "literal")
                literalsCnt++;

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

            if (isMethodDeclaration && lexeme.Kind == LexemeKind.Directive && lexeme.LexemeText == "[out]")
                outCnt++;

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
            OutFrequency = 1d * outCnt / lexemes.Count,
            LiteralFrequency = 1d * literalsCnt / lexemes.Count,
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
        if (Settings.PrintDebugInfo)
            Console.WriteLine($"cosD={codesCosDistance}, strLitD={stringLiteralsDistance}, numLitD={numberLiteralsDistance}");

        if (codesCosDistance > 0.5) return 1;

        if (Settings.PrintDebugInfo)
            Console.WriteLine($"First file stylometry:  {dataA}\nSecond file stylometry: {dataB}\n");

        var vecA = new[]
        {
            dataA.LexicalDiversity,
            dataA.OutFrequency * 10,
            dataA.LiteralFrequency * 10,
            dataA.MethodsAvgLength / 100,
            dataA.MethodsMaxStackAvg / 10,
            dataA.MethodsLocalVarsAvgCnt / 10,
        };

        var vecB = new[]
        {
            dataB.LexicalDiversity,
            dataB.OutFrequency * 10,
            dataB.LiteralFrequency * 10,
            dataB.MethodsAvgLength / 100,
            dataB.MethodsMaxStackAvg / 10,
            dataB.MethodsLocalVarsAvgCnt / 10,
        };

        if (Settings.PrintDebugInfo)
            Console.WriteLine($"Vec1: {string.Join(",", vecA.Select(n => Math.Round(n, 2)))}\n" +
                              $"Vec2: {string.Join(",", vecB.Select(n => Math.Round(n, 2)))}\n");
        
        double distance = Vector.EuclidDistance(vecA, vecB);
        
        if (Settings.PrintDebugInfo)
            Console.WriteLine($"Not corrected distance = {distance}");

        if (codesCosDistance < 0.3)
        {
            double correctedDistance = 1 -
               (1d - distance) *
               ((1 - stringLiteralsDistance) / 2 + 0.5) *
               ((1 - numberLiteralsDistance) / 2 + 0.5);
            
            if (Settings.PrintDebugInfo)
                Console.WriteLine($"Corrected distance = {correctedDistance}");

            return correctedDistance;
        }
        
        if (Settings.PrintDebugInfo) Console.WriteLine();

        return distance;
    }
}

/*
N1 (really one cluster) = 131
N2 (wrong one cluster) = 220
N3 (wrong different clusters) = 557
Accuracy = 0.24651248164464024
*/