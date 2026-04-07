public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        var list = new System.Collections.Generic.List<double>(values);

        if (list.Count == 0)
        {
            throw new System.ArgumentException("Input sequence is empty.");
        }

        double mean = System.Linq.Enumerable.Average(list);
        double min = System.Linq.Enumerable.Min(list);
        double max = System.Linq.Enumerable.Max(list);

        list.Sort();

        int count = list.Count;
        double median;

        if (count % 2 == 0)
        {
            int middleIndex = count / 2;
            median = (list[middleIndex - 1] + list[middleIndex]) / 2.0;
        }
        else
        {
            median = list[count / 2];
        }

        return (mean, median, min, max);
    }
}