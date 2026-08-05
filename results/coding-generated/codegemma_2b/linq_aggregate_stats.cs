using System;
using System.Collections.Generic;

namespace SurveyAnalyzer
{
    public static class SurveyAnalyzer
    {
        public static double Mean(IEnumerable<double> values)
        {
            if (values.Count() == 0)
            {
                throw new ArgumentException("Input sequence is empty.");
            }

            return values.Average();
        }

        public static double Median(IEnumerable<double> values)
        {
            var sortedValues = values.OrderBy(v => v).ToArray();
            var count = sortedValues.Length;

            if (count == 0)
            {
                throw new ArgumentException("Input sequence is empty.");
            }

            if (count % 2 == 0)
            {
                var middleIndex = count / 2;
                return (sortedValues[middleIndex - 1] + sortedValues[middleIndex]) / 2;
            }
            else
            {
                var middleIndex = (count - 1) / 2;
                return sortedValues[middleIndex];
            }
        }

        public static double Min(IEnumerable<double> values)
        {
            if (values.Count() == 0)
            {
                throw new ArgumentException("Input sequence is empty.");
            }

            return values.Min();
        }

        public static double Max(IEnumerable<double> values)
        {
            if (values.Count() == 0)
            {
                throw new ArgumentException("Input sequence is empty.");
            }

            return values.Max();
        }

        public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
        {
            return new (Mean(values), Median(values), Min(values), Max(values));
        }
    }
}