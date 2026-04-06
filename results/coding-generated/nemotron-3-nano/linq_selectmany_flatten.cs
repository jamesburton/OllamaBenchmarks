public record Department(string Name, List<string> Employees);

static class StaffDirectory
{
    public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.Generic.IEnumerable<Department> departments)
    {
        return System.Linq.Enumerable.ToList(
            System.Linq.Enumerable.OrderBy(
                System.Linq.Enumerable.SelectMany(departments, d => d.Employees),
                name => name));
    }
}