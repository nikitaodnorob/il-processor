namespace ClusterAnalysis;

public enum DistanceMetric
{
    Jaccard,
    Cosine,
    Stylometry,
}

public static class Settings
{
    public static bool PrintDebugInfo = false;

    public static double MaxClustersDistance = 0.05;

    public static bool IsLexemesFiltering = false;

    public static DistanceMetric DistanceMetric = DistanceMetric.Jaccard;
}
