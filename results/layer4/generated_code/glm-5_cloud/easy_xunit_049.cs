using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface IDataLoader
{
    Task<List<int>> LoadAsync();
}

public class DataProcessor
{
    private readonly IDataLoader _dataLoader;

    public DataProcessor(IDataLoader dataLoader)
    {
        _dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));
    }

    public async Task<double> GetAverageAsync()
    {
        List<int> data = await _dataLoader.LoadAsync();

        if (data == null || data.Count == 0)
        {
            throw new InvalidOperationException("Cannot calculate average of an empty list.");
        }

        return data.Average();
    }
}