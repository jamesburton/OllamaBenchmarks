public record Department(string Name, System.Collections.Generic.List<string> Employees);

public static class StaffDirectory
{
    public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.Generic.IEnumerable<Department> departments)
    {
        var flattened = System.Linq.Enumerable.SelectMany(departments, d => d.Employees);
        var ordered = System.Linq.Enumerable.OrderBy(flattened, e => e);
        var result = System.Linq.Enumerable.ToList(ordered);
        return result;
    }
}