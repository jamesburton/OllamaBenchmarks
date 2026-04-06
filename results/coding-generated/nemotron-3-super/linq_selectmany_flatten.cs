public record Department(string Name, System.Collections.Generic.List<string> Employees);

static class StaffDirectory
{
    public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.Generic.IEnumerable<Department> departments)
    {
        var flattened = System.Linq.Enumerable.SelectMany(departments, d => d.Employees);
        var sorted = System.Linq.Enumerable.OrderBy(flattened, name => name);
        return System.Linq.Enumerable.ToList(sorted);
    }
}