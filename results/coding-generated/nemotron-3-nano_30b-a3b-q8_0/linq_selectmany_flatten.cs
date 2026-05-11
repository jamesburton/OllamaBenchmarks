public record Department(string Name, List<string> Employees);

public static class StaffDirectory
{
    public static List<string> GetAllEmployees(System.Collections.Generic.IEnumerable<Department> departments)
    {
        var flattened = System.Linq.Enumerable.SelectMany(departments, d => d.Employees);
        var ordered = System.Linq.Enumerable.OrderBy(flattened, x => x);
        return new List<string>(ordered);
    }
}