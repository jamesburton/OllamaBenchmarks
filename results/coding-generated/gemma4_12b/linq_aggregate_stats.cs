public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        var list = System.Linq.Enumerable.ToList(values);

        if (list.Count == 0)
        {
            throw new System.ArgumentException("The input sequence cannot be empty.");
        }

        double mean = System.Linq.Enumerable.Average(list);
        double min = System.Linq.Enumerable.Min(list);
        double max = System.Linq.Enumerable.Max(list);

        var sortedList = System.Linq.Enumerable.OrderBy(list, x => x).ToList();
        int count = sortedList.Count;
        double median;

        if (count % 2 == 0)
        {
            median = (sortedList[count