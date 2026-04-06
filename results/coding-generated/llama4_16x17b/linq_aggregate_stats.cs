using System;
using System.Collections.Generic;
using System.Linq;

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
        {
            throw new ArgumentException("Input sequence is empty");
        }

        var array = values.ToArray();
        var mean = array.Average();
        var min = array.Min();
        var max = array.Max();

        Array.Sort(array);
        var count = array.Length;
        double median;
        if (count % 2 == 0)
        {
            median = (array[count / 2 - 1] + array[count / 2]) / 2;
        }
        else
        {
            median = array[count / 2];
        }

        return (mean, median, min, max);
    }
}