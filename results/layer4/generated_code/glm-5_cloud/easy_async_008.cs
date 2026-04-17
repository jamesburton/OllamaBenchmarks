using System.Threading.Tasks;

public static class Transformer
{
    public static async Task<string> TransformAsync(int value)
    {
        int doubled = value * 2;
        return await Task.FromResult(doubled.ToString());
    }
}