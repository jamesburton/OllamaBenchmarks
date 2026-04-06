public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (values == null || !values.Any())
        {
            throw new System.ArgumentException("Input sequence cannot be empty.");
        }

        double mean = values.Average();
        double min = values.Min();
        double max = values.Max();

        var sorted = values.OrderBy(x => x).ToList();
        int count = sorted.Count;
        double median = count % 2 == 1
            ? sorted[count / 2]
            : (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;

        return (mean, median, min, max);
    }
}