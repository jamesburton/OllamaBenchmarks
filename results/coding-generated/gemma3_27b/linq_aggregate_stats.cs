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

        var sortedValues = values.OrderBy(x => x).ToList();
        int count = sortedValues.Count;
        double median;

        if (count % 2 == 0)
        {
            // Even count: average of the two middle elements
            int middleIndex1 = count / 2 - 1;
            int middleIndex2 = count / 2;
            median = (sortedValues[middleIndex1] + sortedValues[middleIndex2]) / 2.0;
        }
        else
        {
            // Odd count: middle element
            int middleIndex = count / 2;
            median = sortedValues[middleIndex];
        }

        return (mean, median, min, max);
    }
}