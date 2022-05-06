using System.Numerics;
using ILLexer;

namespace ClusterAnalysis;

static class Program
{
    private static readonly Dictionary<string, List<Lexeme>> Codes = new();
    private static readonly Dictionary<string, double[]> LexemesBag = new();

    private const double MaxClustersDistance = 0.05;
    
    public static void Main(string[] args)
    {
        var clusters = new List<List<string>>();
        
        foreach (string fileName in ILCodes.Files)
        {
            string ilCode = File.ReadAllText($"../../../../../master-diploma/{fileName}");
            Console.WriteLine($"Processing {fileName}");
            var fileLexemes = Lexer.GetLexemes(ilCode);
            Codes.Add(fileName, fileLexemes);
            clusters.Add(new List<string> {fileName});
        }
        Console.WriteLine("==================");

        CountLexemesBag();

        while (true)
        {
            var nearestClusters = GetNearestClusters(clusters);
            if (nearestClusters == null) break;

            var (clusterA, clusterB) = ((List<string>, List<string>))nearestClusters;

            clusterA.AddRange(clusterB);
            clusters.Remove(clusterB);
        }

        foreach (var cluster in clusters)
        {
            cluster.ForEach(Console.WriteLine);
            Console.WriteLine("-------------------");
        }

        int rightOneClusterCnt = 0;
        int wrongOneClusterCnt = 0;
        int wrongDifferentClustersCnt = 0;
        int rightDifferentClustersCnt = 0;
        
        // check accuracy
        for (int i = 0; i < ILCodes.Files.Count; i++)
        {
            for (int j = i + 1; j < ILCodes.Files.Count; j++)
            {
                string fileName1 = ILCodes.Files[i];
                string fileName2 = ILCodes.Files[j];
                int author1 = int.Parse(fileName1.Substring(
                    fileName1.IndexOf("/author", StringComparison.Ordinal) + 7,
                    1
                ));
                int author2 = int.Parse(fileName2.Substring(
                    fileName2.IndexOf("/author", StringComparison.Ordinal) + 7,
                    1
                ));
                var cluster1 = clusters.Find(c => c.Contains(fileName1));
                var cluster2 = clusters.Find(c => c.Contains(fileName2));

                if (author1 == author2 && cluster1 == cluster2)
                {
                    // Console.ForegroundColor = ConsoleColor.Green;
                    // Console.WriteLine($"{fileName1} {fileName2}");
                    // Console.ResetColor();
                    rightOneClusterCnt++;
                }
                else if (author1 != author2 && cluster1 == cluster2)
                {
                    // Console.ForegroundColor = ConsoleColor.Red;
                    // Console.WriteLine($"{fileName1} {fileName2}");
                    // Console.ResetColor();
                    wrongOneClusterCnt++;
                }
                else if (author1 == author2 && cluster1 != cluster2)
                {
                    // Console.ForegroundColor = ConsoleColor.Yellow;
                    // Console.WriteLine($"{fileName1} {fileName2}");
                    // Console.ResetColor();
                    wrongDifferentClustersCnt++;
                }
                else if (author1 != author2 && cluster1 != cluster2)
                {
                    rightDifferentClustersCnt++;
                }
            }
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n============\n");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"N1 (really one cluster) = {rightOneClusterCnt}");
        Console.WriteLine($"N2 (wrong one cluster) = {wrongOneClusterCnt}");
        Console.WriteLine($"N3 (wrong different clusters) = {wrongDifferentClustersCnt}");
        // Console.WriteLine($"N4 (really different clusters) = {rightDifferentClustersCnt}");

        double accuracy = (4d * rightOneClusterCnt - 2d * wrongOneClusterCnt - wrongDifferentClustersCnt) /
                          (6d * (rightOneClusterCnt + wrongOneClusterCnt + wrongDifferentClustersCnt)) + 1d / 3;
        Console.WriteLine($"Accuracy = {accuracy}");
    }

    private static (List<string>, List<string>)? GetNearestClusters(List<List<string>> clusters)
    {
        (List<string>?, List<string>?, double) nearestClusters = (null, null, double.PositiveInfinity);

        for (int i = 0; i < clusters.Count; i++)
        {
            for (int j = i + 1; j < clusters.Count; j++)
            {
                double distance = GetClustersDistance(clusters[i], clusters[j]);
                if (distance < nearestClusters.Item3)
                {
                    nearestClusters = (clusters[i], clusters[j], distance);
                }
            }
        }

        if (nearestClusters.Item3 > MaxClustersDistance) return null;

        if (nearestClusters.Item1 == null || nearestClusters.Item2 == null) return null;

        return (nearestClusters.Item1, nearestClusters.Item2);
    }

    private static double GetClustersDistance(List<string> clusterA, List<string> clusterB)
    {
        double distanceSum = 0;

        foreach (string fileA in clusterA)
        {
            foreach (string fileB in clusterB)
            {
                distanceSum += GetCodesDistance(Codes[fileA], Codes[fileB], fileA, fileB);
            }
        }

        return distanceSum / clusterA.Count / clusterB.Count;
    }

    private static double GetCodesDistance(List<Lexeme> codeA, List<Lexeme> codeB, string fileNameA, string fileNameB)
    {
        // return GetCodesJaccardDistance(codeA, codeB);
        return GetCodesCosDistance(fileNameA, fileNameB);
    }

    private static double GetCodesJaccardDistance(List<Lexeme> codeA, List<Lexeme> codeB)
    {
        // codeA = CorrectFileLexemes(codeA);
        // codeB = CorrectFileLexemes(codeB);
        
        var codeALexemes = new HashSet<string>();
        var codeBLexemes = new HashSet<string>();
        codeA.ForEach(lexeme => codeALexemes.Add(lexeme.LexemeText));
        codeB.ForEach(lexeme => codeBLexemes.Add(lexeme.LexemeText));
        var codeABLexemes = codeALexemes.Intersect(codeBLexemes);

        int codeABLexemesCount = codeABLexemes.Count();

        return 1d - 1d * codeABLexemesCount / (codeALexemes.Count + codeBLexemes.Count - codeABLexemesCount);
    }

    private static void CountLexemesBag()
    {
        var lexemes = new HashSet<string>();

        var correctedCodes = new Dictionary<string, List<Lexeme>>();
        foreach (var fileName in Codes.Keys)
        {
            correctedCodes.Add(fileName, CorrectFileLexemes(Codes[fileName]));
            // correctedCodes.Add(fileName, Codes[fileName]);
        }

        foreach (var code in correctedCodes)
        {
            code.Value.ForEach(lexeme =>
            {
                if (lexeme.LexemeText.Length > 0) lexemes.Add(lexeme.LexemeText);
            });
        }

        var idf = new Dictionary<string, double>();
        foreach (var lexemeText in lexemes)
        {
            int codesCnt = correctedCodes.Count(
                code => code.Value.Select(lexeme => lexeme.LexemeText).Contains(lexemeText)
            );
            idf[lexemeText] = 1d + Math.Log(1d * correctedCodes.Count / codesCnt);
            // if (idf[lexemeText] < 1.3) Console.WriteLine(lexemeText);
        }

        int allLexemesCnt = lexemes.Count;
        foreach (var code in correctedCodes)
        {
            LexemesBag.Add(code.Key, new double[allLexemesCnt]);
            int lexemeI = 0;
            foreach (var lexemeText in lexemes)
            {
                // count
                LexemesBag[code.Key][lexemeI] = code.Value.Count(lexeme => lexeme.LexemeText == lexemeText);
                // tf
                LexemesBag[code.Key][lexemeI] /= correctedCodes[code.Key].Count;
                // idf
                LexemesBag[code.Key][lexemeI] *= idf[lexemeText];
                lexemeI++;
            }
        }
    }

    private static double GetCodesCosDistance(string fileNameA, string fileNameB)
    {
        var vecA = LexemesBag[fileNameA];
        var vecB = LexemesBag[fileNameB];

        double dotProd = 0;
        double sqA = 0;
        double sqB = 0;
        for (int i = 0; i < vecA.Length; i++)
        {
            dotProd += vecA[i] * vecB[i];
            sqA += vecA[i] * vecA[i];
            sqB += vecB[i] * vecB[i];
        }
        
        // Console.WriteLine(fileNameA + "-" + fileNameB + "=" + (1d - dotProd / (Math.Sqrt(sqA) * Math.Sqrt(sqB))));

        return 1d - dotProd / (Math.Sqrt(sqA) * Math.Sqrt(sqB));
    }

    private static List<Lexeme> CorrectFileLexemes(List<Lexeme> lexemes)
    {
        var lexemesWithoutPunctuation = lexemes.Where(
            lexeme => lexeme.LexemeText is not "{" or "}" or "[" or "]" or "(" or ")" or "," or ""
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