using ILLexer;

namespace ClusterAnalysis;

internal static class Program
{
    private static readonly Dictionary<string, List<Lexeme>> Codes = new();
    private static readonly Dictionary<string, double[]> LexemesBag = new();
    private static readonly Dictionary<string, List<int>> StringLiteralsList = new();
    private static readonly Dictionary<string, List<int>> NumberLiteralsList = new();
    private static readonly Dictionary<string, StylometryCodeData> StylometryDataList = new();

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

        PrepareForCount();

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
            }
        }

        int clustersCnt = clusters.Count;
        int primitiveClustersCnt = clusters.Count(cluster => cluster.Count == 1);
        int correctClustersCnt = clusters.Count(cluster => (
                cluster.Count > 1 && cluster.Select(fileName => int.Parse(fileName.Substring(
                fileName.IndexOf("/author", StringComparison.Ordinal) + 7,
                1
            ))).Distinct().Count() == 1
        ));

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"N1 (pairs really one cluster) = {rightOneClusterCnt}");
        Console.WriteLine($"N2 (pairs wrong one cluster) = {wrongOneClusterCnt}");
        Console.WriteLine($"Clusters count = {clustersCnt}");
        Console.WriteLine($"Primitive clusters count = {primitiveClustersCnt}");
        Console.WriteLine($"Correct clusters count = {correctClustersCnt}");

        double accuracy =
                (1d * rightOneClusterCnt / (rightOneClusterCnt + wrongOneClusterCnt) / 2d) +
                (1d * correctClustersCnt / clustersCnt / 2d) -
                (1d * primitiveClustersCnt / clustersCnt / 3d);
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

        if (nearestClusters.Item3 > Settings.MaxClustersDistance) return null;

        if (nearestClusters.Item1 == null || nearestClusters.Item2 == null) return null;

        return (nearestClusters.Item1, nearestClusters.Item2);
    }

    private static double GetClustersDistance(List<string> clusterA, List<string> clusterB)
    {
        double distanceSum = 0;

        foreach (string fileA in clusterA)
            foreach (string fileB in clusterB)
                distanceSum += GetCodesDistance(fileA, fileB);

        return distanceSum / clusterA.Count / clusterB.Count;
    }

    private static double GetCodesDistance(string fileNameA, string fileNameB)
    {
        if (Settings.DistanceMetric == DistanceMetric.Jaccard)
            return GetCodesJaccardDistance(fileNameA, fileNameB);
        if (Settings.DistanceMetric == DistanceMetric.Cosine)
            return GetCodesCosDistance(fileNameA, fileNameB);
        if (Settings.DistanceMetric == DistanceMetric.Stylometry)
            return GetStylometryDistance(fileNameA, fileNameB);
        return 0;
    }

    private static void PrepareForCount()
    {
        var correctedCodes = new Dictionary<string, List<Lexeme>>();
        foreach (var fileName in Codes.Keys)
            correctedCodes.Add(
                fileName,
                Settings.IsLexemesFiltering ? LexemesFilter.Filter(Codes[fileName]) : Codes[fileName]
            );

        var lexemesText = GetUniqLexemesText(correctedCodes);
        var stringLiterals = GetUniqStringLiterals(correctedCodes).ToList();
        var numberLiterals = GetUniqNumberLiterals(correctedCodes).ToList();

        var idf = GetIDF(lexemesText, correctedCodes);

        FillLexemesBag(lexemesText, correctedCodes, idf);
        FillStringLiteralsList(stringLiterals, correctedCodes);
        FillNumberLiteralsList(numberLiterals, correctedCodes);
        
        foreach (var fileName in Codes.Keys)
            StylometryDataList.Add(fileName, GetStylometryData(fileName));
    }

    private static HashSet<string> GetUniqLexemesText(Dictionary<string, List<Lexeme>> codes)
    {
        var lexemes = new HashSet<string>();

        foreach (var code in codes)
            foreach (var lexeme in code.Value.Where(lexeme => lexeme.LexemeText.Length > 0))
                lexemes.Add(lexeme.LexemeText);

        return lexemes;
    }
    
    private static HashSet<string> GetUniqStringLiterals(Dictionary<string, List<Lexeme>> codes)
    {
        var stringLiterals = new HashSet<string>();

        foreach (var code in codes)
            foreach (var lexeme in code.Value.Where(lexeme => lexeme.Kind == LexemeKind.StringLiteral))
                stringLiterals.Add(lexeme.LexemeText);

        return stringLiterals;
    }
    
    private static HashSet<double> GetUniqNumberLiterals(Dictionary<string, List<Lexeme>> codes)
    {
        var numberLiterals = new HashSet<double>();

        foreach (var code in codes)
            foreach (var lexeme in code.Value.Where(lexeme => lexeme.Kind == LexemeKind.NumberLiteral))
                numberLiterals.Add(double.Parse(lexeme.LexemeText));

        return numberLiterals;
    }

    private static Dictionary<string, double> GetIDF(HashSet<string> lexemesText, Dictionary<string, List<Lexeme>> codes)
    {
        var idf = new Dictionary<string, double>();
        foreach (var lexemeText in lexemesText)
        {
            int codesCnt = codes.Count(
                code => code.Value.Select(lexeme => lexeme.LexemeText).Contains(lexemeText)
            );
            idf[lexemeText] = 1d + Math.Log(1d * codes.Count / codesCnt);
            // if (idf[lexemeText] < 1.3) Console.WriteLine(lexemeText);
        }

        return idf;
    }

    private static void FillLexemesBag(
        HashSet<string> lexemesText,
        Dictionary<string, List<Lexeme>> codes,
        Dictionary<string, double> idf
    ) {
        foreach (var code in codes)
        {
            LexemesBag.Add(code.Key, new double[lexemesText.Count]);
            int lexemeI = 0;
            foreach (var lexemeText in lexemesText)
            {
                // count
                LexemesBag[code.Key][lexemeI] = code.Value.Count(lexeme => lexeme.LexemeText == lexemeText);
                // tf
                LexemesBag[code.Key][lexemeI] /= codes[code.Key].Count;
                // idf
                LexemesBag[code.Key][lexemeI] *= idf[lexemeText];
                lexemeI++;
            }
        }
    }

    private static void FillStringLiteralsList(List<string> stringLiterals, Dictionary<string, List<Lexeme>> codes)
    {
        foreach (var code in codes)
        {
            StringLiteralsList.Add(code.Key, new List<int>());
            foreach (var lexeme in code.Value.Where(lexeme => lexeme.Kind == LexemeKind.StringLiteral))
                StringLiteralsList[code.Key].Add(stringLiterals.IndexOf(lexeme.LexemeText));
        }
    }
    
    private static void FillNumberLiteralsList(List<double> numberLiterals, Dictionary<string, List<Lexeme>> codes)
    {
        foreach (var code in codes)
        {
            NumberLiteralsList.Add(code.Key, new List<int>());
            foreach (var lexeme in code.Value.Where(lexeme => lexeme.Kind == LexemeKind.NumberLiteral))
                NumberLiteralsList[code.Key].Add(numberLiterals.IndexOf(double.Parse(lexeme.LexemeText)));
        }
    }

    private static double GetCodesJaccardDistance(string fileNameA, string fileNameB)
    {
        var codeA = Codes[fileNameA];
        var codeB = Codes[fileNameB];
        
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

    private static double GetCodesCosDistance(string fileNameA, string fileNameB)
    {
        var vecA = LexemesBag[fileNameA];
        var vecB = LexemesBag[fileNameB];

        // Console.WriteLine(fileNameA + "-" + fileNameB + "=" + Vector.CosDistance(vecA, vecB));
        
        return Vector.CosDistance(vecA, vecB);
    }

    private static double GetStylometryDistance(string fileNameA, string fileNameB)
    {
        var dataA = StylometryDataList[fileNameA];
        var dataB = StylometryDataList[fileNameB];

        var stringLiteralsVecA = StringLiteralsList[fileNameA];
        var stringLiteralsVecB = StringLiteralsList[fileNameB];
        var numberLiteralsVecA = NumberLiteralsList[fileNameA];
        var numberLiteralsVecB = NumberLiteralsList[fileNameB];

        // Console.WriteLine(fileNameA + "-" + fileNameB + "=" + Stylometry.CalcDistance(dataA, dataB, GetCodesCosDistance(fileNameA, fileNameB)));

        return Stylometry.CalcDistance(
            dataA,
            dataB,
            GetCodesCosDistance(fileNameA, fileNameB),
            Vector.CosDistance(stringLiteralsVecA, stringLiteralsVecB),
            Vector.CosDistance(numberLiteralsVecA, numberLiteralsVecB)
        );
    }

    private static StylometryCodeData GetStylometryData(string fileName)
    {
        return Stylometry.CalcStylometryData(Codes[fileName]);
    }

    private static Func<string, TResult> Cache<TResult>(Func<string, TResult> getter)
    {
        var cache = new Dictionary<string, TResult>();

        return fileName =>
        {
            if (!cache.ContainsKey(fileName)) cache.Add(fileName, getter(fileName));
            return cache[fileName];
        };
    }
}