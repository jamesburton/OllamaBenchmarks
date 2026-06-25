public record Department(string Name, List<string> Employees);

public static class StaffDirectory
{
    public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.Generic.IEnumerable<Department> departments)
    {
        return departments
            .SelectMany(d => d.Employees)
            .OrderBy(name => name)
            .ToList();
    }
}