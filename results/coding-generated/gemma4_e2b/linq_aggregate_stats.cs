public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        if (values == null || values.Count() == 0)
        {
            throw new System.ArgumentException("Input sequence cannot be empty.");
        }

        // Convert to list for sorting and easier access
        System.Collections.Generic.List<double> sortedValues = new System.Collections.Generic.List<double>(values);
        sortedValues.Sort();

        int count = sortedValues.Count;
        double mean = 0;
        double median = 0;
        double min = sortedValues[0];
        double max = sortedValues[count - 1];

        // Compute Mean (using manual iteration since we cannot use System.Linq)
        double sum = 0;
        for (int i = 0; i < count; i++)
        {
            sum += sortedValues[i];
        }
        mean = sum / count;

        // Compute Median
        if (count % 2 != 0)
        {
            // Odd count: middle element
            median = sortedValues[count / 2];
        }
        else
        {
            // Even count: average of the two middle elements
            int middle1 = count / 2 - 1;
            int middle2 = count / 2;
            median = (sortedValues[middle1] + sortedValues[middle2]) / 2.0;
        }

        // Min and Max are already found during the sorting/list creation, but we ensure they are correct
        min = sortedValues[0];
        max = sortedValues[count - 1];

        return (mean, median, min, max);
    }
}