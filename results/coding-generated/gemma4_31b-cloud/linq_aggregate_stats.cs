public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        if (values == null || !values.Any())
        {
            throw new System.ArgumentException("The input sequence cannot be empty.");
        }

        double mean = values.Average();
        double min = values.Min();
        double max = values.Max();

        var sortedValues = values.OrderBy(v => v).ToList();
        int count = sortedValues.Count;
        double median;

        if (count % 2 == 0)
        {
            median = (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
        }
        else
        {
            median = sortedValues[count / 2];
        }

        return (mean, median, min, max);
    }
}