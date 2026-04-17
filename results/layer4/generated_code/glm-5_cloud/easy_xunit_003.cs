public interface IGreeter
{
    string Greet(string name);
}

public class FormalGreeter : IGreeter
{
    public string Greet(string name)
    {
        return $"Good day, {name}.";
    }
}