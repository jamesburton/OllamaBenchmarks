public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (values == null || !values.Any())
        {
            throw new ArgumentException("Input sequence cannot be empty or null.");
        }

        double mean = values.Average();
        double min = values.Min();
        double max = values.Max();

        double median;
        var sortedValues = values.OrderBy(v => v).ToArray();
        int count = sortedValues.Length;
        int mid = count / 2;

        if (count % 2 == 1)
        {
            median = sortedValues[mid];
        }
        else
        {
            median = (sortedValues[mid - 1] + sortedValues[mid]) / 2.0;
        }

        return (mean, median, min, max);
    }
}