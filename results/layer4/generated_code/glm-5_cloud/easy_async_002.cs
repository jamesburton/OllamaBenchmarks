using System.Threading.Tasks;

public static class Greeter
{
    public static async Task<string> GetGreetingAsync(string name)
    {
        return await Task.FromResult($"Hello, {name}!");
    }
}