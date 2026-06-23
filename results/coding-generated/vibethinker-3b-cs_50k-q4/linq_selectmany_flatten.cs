public record Department(string Name, List<string> Employees);
public static class StaffDirectory {
    public static List<string> GetAllEmployees(IEnumerable<Department> departments)
    {
        return SelectMany(departments,
            d => d.Employees.Select(e => e),
            OrderBy(e => e))
            .ToList();
    }
}