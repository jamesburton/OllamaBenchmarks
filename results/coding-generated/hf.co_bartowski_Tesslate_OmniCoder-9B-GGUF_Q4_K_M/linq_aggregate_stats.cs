public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
        {
            throw new ArgumentException("Input sequence cannot be empty.");
        }

        var sortedValues = values.OrderBy(x => x).ToList();
        double mean = values.Average();
        double min = values.Min();
        double max = values.Max();
        double median;

        int count = sortedValues.Count;
        if (count % 2 == 1)
        {
            median = sortedValues[count / 2];
        }
        else
        {
            int midIndex = count / 2;
            median = (sortedValues[midIndex - 1] + sortedValues[midIndex]) / 2.0;
        }

        return (mean, median, min, max);
    }
}