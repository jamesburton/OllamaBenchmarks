public record Department(string Name, System.Collections.Generic.List<string> Employees);

public static class StaffDirectory
{
    public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.Generic.IEnumerable<Department> departments)
    {
        return System.Linq.Enumerable.ToList(
            System.Linq.Enumerable.OrderBy(
                System.Linq.Enumerable.SelectMany(departments, d => d.Employees),
                e => e
            )
        );
    }
}