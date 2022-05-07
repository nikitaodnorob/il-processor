namespace ClusterAnalysis;

public class Vector
{
    public static double CosDistance(double[] vecA, double[] vecB)
    {
        double dotProd = 0;
        double sqA = 0;
        double sqB = 0;
        for (int i = 0; i < vecA.Length; i++)
        {
            dotProd += vecA[i] * vecB[i];
            sqA += vecA[i] * vecA[i];
            sqB += vecB[i] * vecB[i];
        }

        return 1d - dotProd / (Math.Sqrt(sqA) * Math.Sqrt(sqB));
    }

    public static double[] Normalize(double[] vec)
    {
        double sqSum = 0;
        for (int i = 0; i < vec.Length; i++) sqSum += vec[i] * vec[i];

        double[] res = new double[vec.Length];
        for (int i = 0; i < vec.Length; i++) res[i] = vec[i] / sqSum;

        return res;
    }
}
