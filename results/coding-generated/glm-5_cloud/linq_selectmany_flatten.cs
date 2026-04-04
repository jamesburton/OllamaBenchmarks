public record Department
{
    public string Name { get; init; }
    public List<string> Employees { get; init; }
}

public static class StaffDirectory
{
    public static List<string> GetAllEmployees(IEnumerable<Department> departments)
    {
        return departments
            .SelectMany(d => d.Employees)
            .OrderBy(name => name)
            .ToList();
    }
}