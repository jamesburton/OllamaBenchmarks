public class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (values.Count == 0)
            throw new ArgumentException("Input sequence is empty.");
        var mean = values.Average();
        var min = values.Min();
        var max = values.Max();
        var sorted = values.OrderBy(x => x).ToList();
        double median;
        if (sorted.Count % 2 == 1)
            median = sorted[sorted.Count / 2];
        else
            median = (sorted[sorted.Count / 2 - 1] + sorted[sorted.Count / 2]) / 2;
        return (mean, median, min, max);
    }
}