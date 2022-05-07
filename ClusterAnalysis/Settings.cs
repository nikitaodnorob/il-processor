namespace ClusterAnalysis;

public enum DistanceMetric
{
    Jaccard,
    Cosine,
}

public static class Settings
{
    public static double MaxClustersDistance = 0.05;

    public static bool IsLexemesFiltering = false;

    public static DistanceMetric DistanceMetric = DistanceMetric.Jaccard;
}
