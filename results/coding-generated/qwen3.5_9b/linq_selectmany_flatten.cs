public record Department(string Name, List<string> Employees);

internal static class StaffDirectory
{
    public static List<string> GetAllEmployees(IEnumerable<Department> departments)
        => departments.SelectMany(d => d.Employees).OrderBy(e => e).ToList();
}