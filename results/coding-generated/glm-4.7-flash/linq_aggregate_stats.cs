public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
        {
            throw new ArgumentException("Input sequence cannot be empty.");
        }

        double mean = values.Average();
        double min = values.Min();
        double max = values.Max();

        var sortedValues = values.OrderBy(v => v).ToList();
        int count = sortedValues.Count;
        int mid = count / 2;

        double median;
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