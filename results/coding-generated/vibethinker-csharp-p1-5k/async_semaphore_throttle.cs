static void Main(string[] args)
{
            Console.WriteLine("Running the following C# method.");
            var result = ThrottledProcessor.ProcessAllAsync(new List<Func<CancellationToken, Task<string>>> {
                new Func<CancellationToken, Task<string>>(ct => new Task<string>(t => t.ToString()).Wait(ct))
            });
            Console.WriteLine(result);
        }