public static class SurveyAnalyzer
    {
        public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
        {
            if (values == null || !values.Any())
            {
                throw new System.ArgumentException("Input sequence cannot be empty.");
            }

            double mean = System.Linq.Enumerable.Average(values);
            double min = System.Linq.Enumerable.Min(values);
            double max = System.Linq.Enumerable.Max(values);

            // For Median
            // Need to sort. Since we can't use LINQ OrderBy without fully qualifying, 
            // and we need to sort the collection.
            // System.Linq.Enumerable.OrderBy returns IEnumerable.
            // We need to convert to array or list to sort easily.
            // Or use System.Linq.Enumerable.OrderBy.

            // Wait, if I use values.OrderBy(...), I need to qualify it.
            // Also, Any() needs qualification.

            // Let's refine the logic.
            // To compute median, we need the sorted sequence.
            // System.Linq.Enumerable.OrderBy(values, x => x)
            // But OrderBy returns IEnumerable.
            // We need to access the count.

            // Let's convert to array for sorting to make it easier to index.
            // System.Linq.Enumerable.ToArray(values)

            double[] sortedValues = System.Linq.Enumerable.ToArray(values);
            int count = sortedValues.Length;
            double median;

            if (count % 2 == 1)
            {
                median = sortedValues[count / 2];
            }
            else
            {
                int midIndex = count / 2;
                median = (sortedValues[midIndex - 1] + sortedValues[midIndex]) / 2.0;
            }

            return (Mean: mean, Median: median, Min: min, Max: max);
        }
    }