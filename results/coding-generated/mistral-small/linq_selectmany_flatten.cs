public record Department(string Name, List<string> Employees);

public static class StaffDirectory
{
    public static List<string> GetAllEmployees(IEnumerable<Department> departments)
    {
        return departments.SelectMany(dept => dept.Employees).OrderBy(name => name).ToList();
    }
}