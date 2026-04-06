public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (values == null)
        {
            throw new ArgumentException("Input sequence cannot be null.");
        }

        if (values.Count() == 0)
        {
            throw new ArgumentException("Input sequence cannot be empty.");
        }

        double[] sortedValues = values.ToArray();
        Array.Sort(sortedValues);

        double mean = values.Average();
        double median;
        int count = sortedValues.Length;

        if (count % 2 != 0)
        {
            median = sortedValues[count / 2];
        }
        else
        {
            median = (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
        }

        double min = sortedValues[0];
        double max = sortedValues[count - 1];

        return (mean, median, min, max);
    }
}