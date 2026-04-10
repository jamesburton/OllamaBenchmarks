public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (values == null || !values.Any())
        {
            throw new ArgumentException("Input sequence cannot be empty.");
        }

        double mean = values.Average();
        double min = values.Min();
        double max = values.Max();

        var sorted = values.OrderBy(v => v).ToList();
        int count = sorted.Count;

        double median;
        if (count % 2 == 1)
        {
            median = sorted[count / 2];
        }
        else
        {
            int middleIndex = count / 2;
            median = (sorted[middleIndex - 1] + sorted[middleIndex]) / 2.0;
        }

        return (mean, median, min, max);
    }
}