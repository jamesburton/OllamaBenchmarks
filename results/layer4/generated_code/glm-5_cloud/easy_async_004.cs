using System.Threading.Tasks;

public static class Calculator
{
    public static async Task<int> AddAsync(int a, int b)
    {
        return await Task.FromResult(a + b);
    }
}