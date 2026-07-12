using System;
using System.Collections.Generic;
using System.Linq;

static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
        {
            throw new ArgumentException("Input sequence is empty.");
        }

        var sortedValues = values.OrderBy(v => v).ToList();
        int count = sortedValues.Count;
        double mean = values.Average();
        double min = values.Min();
        double max = values.Max();

        double median = count % 2 == 1
            ? sortedValues[count / 2]
            : (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;

        return (mean, median, min, max);
    }
}