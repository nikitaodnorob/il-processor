namespace ClusterAnalysis;

public struct StylometryCodeData
{
    public double MethodsAvgLength;
    public double MethodsLocalVarsAvgCnt;
    public double MethodsMaxStackAvg;
    public double LexicalDiversity;

    public override string ToString()
    {
        return $"MethodsAvgL={Math.Round(MethodsAvgLength, 2).ToString(),-5}, " +
               $"MethodsVarsAvgCnt={Math.Round(MethodsLocalVarsAvgCnt, 2).ToString(),-5}, " +
               $"MethodsMaxStackAvg={Math.Round(MethodsMaxStackAvg, 2).ToString(),-5}, " +
               $"LexDiv={Math.Round(LexicalDiversity, 4).ToString(),-6}";
    }
}
