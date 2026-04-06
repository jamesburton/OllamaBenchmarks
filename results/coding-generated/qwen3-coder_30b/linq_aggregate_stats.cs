using System;
using System.Collections.Generic;
using System.Linq;

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (values == null || !values.Any())
        {
            throw new ArgumentException();
        }

        var valuesList = values.ToList();
        var mean = valuesList.Average();
        var min = valuesList.Min();
        var max = valuesList.Max();
        var median = CalculateMedian(valuesList);

        return (mean, median, min, max);
    }

    private static double CalculateMedian(List<double> sortedValues)
    {
        sortedValues.Sort();
        int count = sortedValues.Count;

        if (count % 2 == 1)
        {
            return sortedValues[count / 2];
        }
        else
        {
            int midIndex1 = (count / 2) - 1;
            int midIndex2 = count / 2;
            return (sortedValues[midIndex1] + sortedValues[midIndex2]) / 2.0;
        }
    }
}