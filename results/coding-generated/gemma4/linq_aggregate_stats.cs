public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        // Check for empty input
        if (!values.Any())
        {
            throw new ArgumentException("Input sequence cannot be empty.", nameof(values));
        }

        // Convert to list to allow sorting and multiple passes
        var sortedValues = values.ToList();
        int count = sortedValues.Count;

        // 1. Compute Mean (Requires System.Linq)
        double mean = sortedValues.Average();

        // 2. Compute Min (Requires System.Linq)
        double min = sortedValues.Min();

        // 3. Compute Max (Requires System.Linq)
        double max = sortedValues.Max();

        // 4. Compute Median
        // Sort the list for median calculation
        sortedValues.Sort();

        double median;
        if (count % 2 != 0)
        {
            // Odd count: Median is the middle element
            median = sortedValues[count / 2];
        }
        else
        {
            // Even count: Median is the average of the two middle elements
            int middleIndex1 = count / 2 - 1;
            int middleIndex2 = count / 2;
            median = (sortedValues[middleIndex1] + sortedValues[middleIndex2]) / 2.0;
        }

        // Return the named tuple
        return (Mean: mean, Median: median, Min: min, Max: max);
    }
}