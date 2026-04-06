using System;
using System.Collections.Generic;
using System.Linq;

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        var valuesList = values.ToList();

        if (!valuesList.Any())
        {
            throw new ArgumentException("The input sequence cannot be empty.");
        }

        double mean = valuesList.Average();
        double min = valuesList.Min();
        double max = valuesList.Max();

        var sortedValues = valuesList.OrderBy(x => x).ToList();
        int count = sortedValues.Count;
        double median;

        if (count % 2 != 0)
        {
            median = sortedValues[count / 2];
        }
        else
        {
            median = (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
        }

        return (mean, median, min, max);
    }
}