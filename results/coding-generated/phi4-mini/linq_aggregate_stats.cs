public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
            throw new ArgumentException("Input sequence cannot be empty.");

        var sortedValues = values.OrderBy(v => v).ToList();
        int count = sortedValues.Count;
        double mean = values.Average();

        double min = sortedValues.Min();
        double max = sortedValues.Max();

        double median;
        if (count % 2 == 0)
        {
            // Even number of elements, average the two middle numbers
            var mid1 = count / 2 - 1;
            var mid2 = mid1 + 1;
            median = (sortedValues[mid1] + sortedValues[mid2]) / 2.0;
        }
        else
        {
            // Odd number of elements, take the middle one
            median = sortedValues[count / 2];
        }

        return (mean, median, min, max);
    }
}