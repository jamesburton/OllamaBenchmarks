public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        double[] arr = System.Linq.Enumerable.ToArray(values);

        if (arr.Length == 0)
        {
            throw new System.ArgumentException("Sequence is empty");
        }

        double mean = System.Linq.Enumerable.Average(arr);
        double min = System.Linq.Enumerable.Min(arr);
        double max = System.Linq.Enumerable.Max(arr);

        System.Array.Sort(arr);

        double median;
        int count = arr.Length;

        if (count % 2 == 0)
        {
            median = (arr[count / 2 - 1] + arr[count / 2]) / 2.0;
        }
        else
        {
            median = arr[count / 2];
        }

        return (Mean: mean, Median: median, Min: min, Max: max);
    }
}