public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        var list = values.ToList();
        if (list.Count == 0) throw new ArgumentException("Input sequence cannot be empty.");
        var mean = list.Average();
        var min = list.Min();
        var max = list.Max();
        var sorted = list.OrderBy(x => x).ToList();
        double median;
        int count = sorted.Count;
        if (count % 2 == 1)
            median = sorted[count / 2];
        else
            median = (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        return (mean, median, min, max);
    }
}