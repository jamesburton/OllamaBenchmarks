public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
            throw new ArgumentException("Input sequence cannot be empty.");

        double mean = values.Average();
        double min = values.Min();
        double max = values.Max();

        var sorted = values.OrderBy(x => x);
        int count = sorted.Count();
        if (count % 2 == 1)
        {
            double median = sorted.ElementAt(count / 2);
            return (mean, median, min, max);
        }
        else
        {
            double mid1 = sorted.ElementAt(count / 2 - 1);
            double mid2 = sorted.ElementAt(count / 2);
            double median = (mid1 + mid2) / 2;
            return (mean, median, min, max);
        }
    }
}