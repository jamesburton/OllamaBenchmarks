public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        var arr = System.Linq.Enumerable.ToArray(values);
        if (arr.Length == 0)
        {
            throw new System.ArgumentException();
        }

        double mean = System.Linq.Enumerable.Average(arr);
        double min = System.Linq.Enumerable.Min(arr);
        double max = System.Linq.Enumerable.Max(arr);

        System.Array.Sort(arr);
        double median;
        int n = arr.Length;
        if (n % 2 == 1)
        {
            median = arr[n / 2];
        }
        else
        {
            median = (arr[n / 2 - 1] + arr[n / 2]) / 2.0;
        }

        return (mean, median, min, max);
    }
}