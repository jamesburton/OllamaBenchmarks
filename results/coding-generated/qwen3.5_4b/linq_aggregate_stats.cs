public static class SurveyAnalyzer
    {
        public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
        {
            if (!values.Any())
            {
                throw new ArgumentException("Input sequence is empty.");
            }

            double mean = values.Average();
            double min = values.Min();
            double max = values.Max();

            // Median calculation
            double[] sorted = values.ToArray();
            Array.Sort(sorted);
            int count = sorted.Length;
            double median;

            if (count % 2 == 1)
            {
                median = sorted[count / 2];
            }
            else
            {
                median = (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            }

            return (mean, median, min, max);
        }
    }

...