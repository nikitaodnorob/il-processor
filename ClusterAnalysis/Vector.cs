namespace ClusterAnalysis;

public static class Vector
{
    public static double CosDistance(double[] vecA, double[] vecB)
    {
        if (vecA.Length + vecB.Length == 0) return 0;
        if (vecA.Length * vecB.Length == 0) return 1;

        double dotProd = 0;
        double sqA = 0;
        double sqB = 0;
        for (int i = 0; i < Math.Max(vecA.Length, vecB.Length); i++)
        {
            double curA = TryGetByIndex(vecA, i, 0);
            double curB = TryGetByIndex(vecB, i, 0);
            dotProd += curA * curB;
            sqA += curA * curA;
            sqB += curB * curB;
        }

        if (sqA + sqB == 0) return 0;

        return 1d - dotProd / (Math.Sqrt(sqA) * Math.Sqrt(sqB));
    }

    public static double CosDistance(int[] vecA, int[] vecB)
    {
        return CosDistance(vecA.Select(n => n * 1d).ToArray(), vecB.Select(n => n * 1d).ToArray());
    }
    
    public static double CosDistance(List<int> vecA, List<int> vecB)
    {
        return CosDistance(vecA.Select(n => n * 1d).ToArray(), vecB.Select(n => n * 1d).ToArray());
    }

    public static double EuclidDistance(double[] vecA, double[] vecB)
    {
        if (vecA.Length + vecB.Length == 0) return 0;
        if (vecA.Length * vecB.Length == 0) return 1;
        
        double sqSum = 0;
        double n = Math.Max(vecA.Length, vecB.Length);
        for (int i = 0; i < n; i++)
        {
            double curA = TryGetByIndex(vecA, i, 0);
            double curB = TryGetByIndex(vecB, i, 0);
            sqSum += Math.Pow(curA - curB, 2);
        }

        return Math.Sqrt(sqSum);
    }

    public static double[] Normalize(double[] vec)
    {
        double sqSum = 0;
        for (int i = 0; i < vec.Length; i++) sqSum += vec[i] * vec[i];

        double[] res = new double[vec.Length];
        for (int i = 0; i < vec.Length; i++) res[i] = vec[i] / sqSum;

        return res;
    }

    private static T TryGetByIndex<T>(T[] arr, int i, T defaultValue)
    {
        return i < arr.Length ? arr[i] : defaultValue;
    }
}
