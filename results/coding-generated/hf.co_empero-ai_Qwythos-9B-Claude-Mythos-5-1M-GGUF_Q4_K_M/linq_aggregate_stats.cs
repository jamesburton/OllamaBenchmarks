public static class SurveyAnalyzer {
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values) {
        if (values == null || !values.Any()) throw new ArgumentException("Input sequence cannot be empty.");
        var sorted = values.OrderBy(x => x).ToList();
        int count = sorted.Count;
        double median;
        if (count % 2 == 0) {
            median = (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        } else {
            median = sorted[count / 2];
        }
        return (values.Average(), median, values.Min(), values.Max());
    }
}