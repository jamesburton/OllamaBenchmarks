using System;
using System.Collections.Generic;
using System.Linq;

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (values == null || !values.Any())
        {
            throw new ArgumentException("Input sequence cannot be empty or null.");
        }

        var sortedValues = values.OrderBy(v => v).ToList();
        int count = sortedValues.Count;

        // Compute Mean
        double mean = sortedValues.Average();

        // Compute Min and Max
        double min = sortedValues.Min();
        double max = sortedValues.Max();

        // Compute Median
        double median;
        if (count % 2 == 1)
        {
            // Odd count: middle element
            median = sortedValues[count / 2];
        }
        else
        {
            // Even count: average of the two middle elements
            int middleRightIndex = count / 2;
            int middleLeftIndex = middleRightIndex - 1;
            median = (sortedValues[middleLeftIndex] + sortedValues[middleRightIndex]) / 2.0;
        }

        return (mean, median, min, max);
    }
}