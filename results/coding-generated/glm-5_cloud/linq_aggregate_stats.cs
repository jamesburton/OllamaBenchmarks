public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        var valueList = new System.Collections.Generic.List<double>(values);

        if (valueList.Count == 0)
        {
            throw new System.ArgumentException("The input sequence is empty.");
        }

        double mean = System.Linq.Enumerable.Average(valueList);
        double min = System.Linq.Enumerable.Min(valueList);
        double max = System.Linq.Enumerable.Max(valueList);

        valueList.Sort();

        int count = valueList.Count;
        double median;

        if (count % 2 == 0)
        {
            median = (valueList[count / 2 - 1] + valueList[count / 2]) / 2.0;
        }
        else
        {
            median = valueList[count / 2];
        }

        return (mean, median, min, max);
    }
}