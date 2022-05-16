public static class LevenshteinCalculator
{
    private static int LevenshteinDistance(int[] v1, int[] v2)
    {
        int[,] m = new int[v1.Length + 1, v2.Length + 1];

        for (int i = 0; i <= v1.Length; i++) { m[i, 0] = i; }
        for (int j = 0; j <= v2.Length; j++) { m[0, j] = j; }

        for (int i = 1; i <= v1.Length; i++)
        {
            for (int j = 1; j <= v2.Length; j++)
            {
                var replaceCost = v1[i - 1] == v2[j - 1] ? 0 : 2;
                m[i, j] = Math.Min(Math.Min(m[i - 1, j] + 1, m[i, j - 1] + 1), m[i - 1, j - 1] + replaceCost);
            }
        }
        return m[v1.Length, v2.Length];
    }

    public static double VectorsDistance(int[] v1, int[] v2)
    {
        var maxLength = (double)Math.Max(v1.Length, v2.Length);
        if (maxLength == 0) return 0;
        
        return Math.Min(LevenshteinDistance(v1, v2) / maxLength, 1);
    }

    public static double VectorsDistance(List<int> v1, List<int> v2)
    {
        return VectorsDistance(v1.ToArray(), v2.ToArray());
    }
}