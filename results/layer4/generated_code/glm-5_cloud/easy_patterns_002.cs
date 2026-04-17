public record Person(string Name, int Age);

public static class PersonHelper
{
    public static Person WithName(Person person, string newName)
    {
        return person with { Name = newName };
    }
}