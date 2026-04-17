public record Person(string FirstName, string LastName)
{
    public string FullName => $"{FirstName} {LastName}";
}

public class PersonService
{
    public Person CreatePerson(string firstName, string lastName)
    {
        return new Person(firstName, lastName);
    }

    public string GetDisplayName(Person person)
    {
        return person.FullName.ToUpper();
    }
}