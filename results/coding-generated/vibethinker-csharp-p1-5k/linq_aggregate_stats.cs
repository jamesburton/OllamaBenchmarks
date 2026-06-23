static void Main(string[] args)
{
            Console.WriteLine("Running the following C# method.");
            var result = new (double Mean, double Median, double Min, double Max) {
                Mean = 1.0,
                Median = 2.5,
                Min = 1.0,
                Max = 3.0
            };

            Console.WriteLine($"Mean: {result.Mean}");
            Console.WriteLine($"Median: {result.Median}");
            Console.WriteLine($"Min: {result.Min}");
            Console.WriteLine($"Max: {result.Max}");
        }