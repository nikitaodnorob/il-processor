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

    public static double MaxClustersDistance = 0.2;

    public static bool IsLexemesFiltering = true;

    public static DistanceMetric DistanceMetric = DistanceMetric.Stylometry;
}

/*
 * Best max distance:
 *  Jaccard: 0.25
 *  Cosine: 0.1 (without filtering), 0.025 (with filtering)
 *  Stylometry: 0.2
 */