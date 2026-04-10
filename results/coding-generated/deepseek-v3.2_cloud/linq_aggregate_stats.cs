public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
        {
            throw new ArgumentException("Input sequence cannot be empty.");
        }

        double min = values.Min();
        double max = values.Max();
        double mean = values.Average();

        List<double> sorted = values.OrderBy(x => x).ToList();
        int count = sorted.Count;
        double median;

        if (count % 2 == 1)
        {
            median = sorted[count / 2];
        }
        else
        {
            median = (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }

        return (mean, median, min, max);
    }
}