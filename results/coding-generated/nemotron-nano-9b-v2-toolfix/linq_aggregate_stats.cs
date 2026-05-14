public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
            throw new ArgumentException("Input sequence cannot be empty.");

        var list = values.ToList();
        double mean = list.Average();
        double min = list.Min();
        double max = list.Max();
        list.Sort();
        int count = list.Count;
        double median = count % 2 == 1 
            ? list[count / 2] 
            : (list[count / 2 - 1] + list[count / 2]) / 2;
        return (mean, median, min, max);
    }
}