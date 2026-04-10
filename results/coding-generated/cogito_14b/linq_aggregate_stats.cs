public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
            throw new ArgumentException("Input sequence cannot be empty.");

        var sortedValues = values.OrderBy(v => v).ToList();
        int count = sortedValues.Count;
        double median;

        if (count % 2 == 0)
            median = (sortedValues[count/2 - 1] + sortedValues[count/2]) / 2;
        else
            median = sortedValues[count/2];

        return (
            Mean: values.Average(),
            Median: median,
            Min: values.Min(),
            Max: values.Max()
        );
    }
}