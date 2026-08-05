public record Department(string Name, List<string> Employees);

public static class StaffDirectory
{
    public static List<string> GetAllEmployees(IEnumerable<Department> departments) =>
        departments.SelectMany(d => d.Employees).OrderBy(name => name).ToList();
}