public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        if (values == null)
        {
            throw new System.ArgumentException("Input sequence cannot be null.");
        }

        var valuesArray = new System.Collections.Generic.List<double>(values);
        if (valuesArray.Count == 0)
        {
            throw new System.ArgumentException("Input sequence cannot be empty.");
        }

        double mean = valuesArray.Average();
        double min = valuesArray.Min();
        double max = valuesArray.Max();

        var sortedValues = valuesArray.ToArray();
        System.Array.Sort(sortedValues);

        double median;
        int count = sortedValues.Length;
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