public record Department(string Name, System.Collections.Generic.List<string> Employees);

public static class StaffDirectory
{
    public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.Generic.IEnumerable<Department> departments)
    {
        return System.Linq.Enumerable.SelectMany(departments, dept => dept.Employees)
            .OrderBy(emp => emp)
            .ToList();
    }
}