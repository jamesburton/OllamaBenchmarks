public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        var array = System.Linq.Enumerable.ToArray(values);
        if (array.Length == 0)
            throw new ArgumentException("Sequence cannot be empty.");
        double mean = System.Linq.Enumerable.Average(array);
        double min = System.Linq.Enumerable.Min(array);
        double max = System.Linq.Enumerable.Max(array);

        var sorted = System.Linq.Enumerable.OrderBy(array, (double x) => x);
        var sortedArray = System.Linq.Enumerable.ToArray(sorted);
        int count = sortedArray.Length;
        double median;
        if (count % 2 == 1)
            median = sortedArray[count / 2];
        else
            median = (sortedArray[count / 2 - 1] + sortedArray[count / 2]) / 2.0;

        return (mean, median, min, max);
    }
}