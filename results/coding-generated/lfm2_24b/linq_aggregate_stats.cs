public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        var list = values.ToList();
        if (list.Count() == 0)
            throw new ArgumentException("Input sequence cannot be empty.");

        double mean = list.Average();
        double min = list.Min();
        double max = list.Max();

        var sorted = list.OrderBy(x => x).ToList();
        int count = sorted.Count;
        double median;
        if (count % 2 == 1)
            median = sorted[count / 2];
        else
            median = (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;

        return (mean, median, min, max);
    }
}