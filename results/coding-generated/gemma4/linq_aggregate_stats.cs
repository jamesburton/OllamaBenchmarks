public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        var list = values.ToList();

        if (list.Count == 0)
        {
            throw new ArgumentException("Input sequence cannot be empty.");
        }

        double mean = list.Average();
        double min = list.Min();
        double max = list.Max();

        list.Sort();
        double median;
        int count = list.Count;

        if (count % 2 != 0)
        {
            median = list[count / 2];
        }
        else
        {
            int middle1 = count / 2 - 1;
            int middle2 = count / 2;
            median = (list[middle1] + list[middle2]) / 2.0;
        }

        return (Mean: mean, Median: median, Min: min, Max: max);
    }
}