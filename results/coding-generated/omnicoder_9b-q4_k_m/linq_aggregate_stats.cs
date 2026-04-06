public static class SurveyAnalyzer
    {
        public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
        {
            if (values == null) throw new System.ArgumentException("values cannot be null");
            var list = System.Linq.Enumerable.ToList(values);
            if (list.Count == 0) throw new System.ArgumentException("values cannot be empty");

            double mean = System.Linq.Enumerable.Average(list);
            double min = System.Linq.Enumerable.Min(list);
            double max = System.Linq.Enumerable.Max(list);

            list.Sort();
            int count = list.Count;
            double median;
            if (count % 2 == 1)
            {
                median = list[count / 2];
            }
            else
            {
                median = (list[count / 2 - 1] + list[count / 2]) / 2.0;
            }

            return (Mean: mean, Median: median, Min: min, Max: max);
        }
    }