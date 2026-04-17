using System;
using System.Threading.Tasks;

public interface IFileReader
{
    Task<string> ReadAllTextAsync(string path);
}

public class FileProcessor
{
    private readonly IFileReader _fileReader;

    public FileProcessor(IFileReader fileReader)
    {
        _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
    }

    public async Task<int> CountLinesAsync(string path)
    {
        string content = await _fileReader.ReadAllTextAsync(path);

        if (string.IsNullOrEmpty(content))
        {
            return 0;
        }

        return content.Split('\n').Length;
    }
}