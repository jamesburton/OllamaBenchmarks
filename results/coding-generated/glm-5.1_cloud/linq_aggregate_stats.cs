public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        var list = values.ToList();

        if (list.Count == 0)
        {
            throw new ArgumentException("Sequence is empty.");
        }

        double mean = list.Average();
        double min = list.Min();
        double max = list.Max();

        list.Sort();

        int count = list.Count;
        double median;

        if (count % 2 == 0)
        {
            median = (list[count / 2 - 1] + list[count / 2]) / 2.0;
        }
        else
        {
            median = list[count / 2];
        }

        return (mean, median, min, max);
    }
}