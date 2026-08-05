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

        double[] sortedValues = values.OrderBy(v => v).ToArray();
        int count = sortedValues.Length;
        double median;

        if (count % 2 == 1)
        {
            median = sortedValues[count / 2];
        }
        else
        {
            median = (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
        }

        return (mean, median, min, max);
    }
}