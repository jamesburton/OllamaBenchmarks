public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
            throw new ArgumentException("Input sequence is empty");

        var sortedValues = values.OrderBy(v => v).ToList();
        var count = sortedValues.Count;

        var mean = values.Average();
        var min = sortedValues.Min();
        var max = sortedValues.Max();
        var median = count % 2 == 0 ? (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0 : sortedValues[count / 2];

        return (mean, median, min, max);
    }
}