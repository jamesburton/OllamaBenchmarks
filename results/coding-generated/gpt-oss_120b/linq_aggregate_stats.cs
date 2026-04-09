public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        if (values == null)
            throw new System.ArgumentNullException(nameof(values));

        // Materialize the sequence to allow multiple passes and sorting
        var list = new System.Collections.Generic.List<double>(values);
        if (list.Count == 0)
            throw new System.ArgumentException("The input sequence must contain at least one element.");

        double mean = System.Linq.Enumerable.Average(values);
        double min = System.Linq.Enumerable.Min(values);
        double max = System.Linq.Enumerable.Max(values);

        // Ensure the list is sorted for median calculation
        list.Sort();

        double median;
        int count = list.Count;
        if (count % 2 == 1) // odd number of elements
        {
            median = list[count / 2];
        }
        else // even number of elements
        {
            median = (list[count / 2 - 1] + list[count / 2]) / 2.0;
        }

        return (Mean: mean, Median: median, Min: min, Max: max);
    }
}