public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
            throw new ArgumentException("Input sequence cannot be empty.");

        var valuesArray = values.ToArray();
        double min = valuesArray.Min();
        double max = valuesArray.Max();
        double mean = valuesArray.Average();

        double median;
        if (valuesArray.Length % 2 == 0)
        {
            int mid1 = valuesArray.Length / 2 - 1;
            int mid2 = valuesArray.Length / 2;
            median = (valuesArray[mid1] + valuesArray[mid2]) / 2.0;
        }
        else
        {
            median = valuesArray[valuesArray.Length / 2];
        }

        return (mean, median, min, max);
    }
}