using System.Linq;

public static class SurveyAnalyzer {
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values) {
        if(values.Count() == 0)
            throw new ArgumentException("Input sequence is empty.");

        var mean = values.Average();
        var min = values.Min();
        var max = values.Max();

        int count = values.Count();
        if(count % 2 != 0)
            median = (values.OrderBy(x => x).Skip((count - 1) / 2).Take(1).First() + 
                      values.OrderBy(x => x).Skip(count / 2).Take(1).First()) / 2.0;
        else
            median = (values.OrderBy(x => x).Skip(count / 2).Take(1).First() +
                       values.OrderBy(x => x).Skip((count - 1) / 2).Take(1).First()) / 2.0;

        return (mean, median, min, max);
    }
}