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

    public static double MaxClustersDistance = 0.25;

    public static bool IsLexemesFiltering = true;

    public static DistanceMetric DistanceMetric = DistanceMetric.Stylometry;
}

/*
 * Best max distance:
 *  Jaccard: 0.2 (without filtering), 0.15 (with filtering)
 *  Cosine: 0.1 (without filtering), 0.0375 (with filtering)
 *  Stylometry: 0.25 with filtering
 */