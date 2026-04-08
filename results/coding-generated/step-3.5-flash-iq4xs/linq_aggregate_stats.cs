public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        double[] array = System.Linq.Enumerable.ToArray(values);
        if (array.Length == 0)
            throw new System.ArgumentException("Sequence is empty.");

        double mean = System.Linq.Enumerable.Average(array);
        double min = System.Linq.Enumerable.Min(array);
        double max = System.Linq.Enumerable.Max(array);

        System.Array.Sort(array);
        int count = array.Length;
        double median;
        if (count % 2 == 1)
        {
            median = array[count / 2];
        }
        else
        {
            median = (array[count / 2 - 1] + array[count / 2]) / 2.0;
        }

        return (mean, median, min, max);
    }
}