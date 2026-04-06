using System.Collections.Generic;
using System.Linq;

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        // Convert to list to allow multiple passes (sorting, counting)
        var list = values.ToList();

        if (list.Count == 0)
        {
            throw new ArgumentException("Input sequence cannot be empty.", nameof(values));
        }

        // 1. Compute Mean
        double mean = list.Average();

        // 2. Compute Min
        double min = list.Min();

        // 3. Compute Max
        double max = list.Max();

        // 4. Compute Median
        // Create a sorted copy for median calculation
        var sortedList = list.OrderBy(x => x).ToList();
        int count = sortedList.Count;
        double median;

        if (count % 2 != 0)
        {
            // Odd count: Median is the middle element
            median = sortedList[count / 2];
        }
        else
        {
            // Even count: Median is the average of the two middle elements
            int middleIndex1 = count / 2 - 1;
            int middleIndex2 = count / 2;
            median = (sortedList[middleIndex1] + sortedList[middleIndex2]) / 2.0;
        }

        // 5. Return the named tuple
        return (Mean: mean, Median: median, Min: min, Max: max);
    }
}