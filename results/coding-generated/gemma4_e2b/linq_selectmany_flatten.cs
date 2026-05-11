public record Department(string Name, List<string> Employees);

public static class StaffDirectory
{
    public static List<string> GetAllEmployees(IEnumerable<Department> departments)
    {
        // Flatten all employees lists using SelectMany
        var allEmployees = departments.SelectMany(d => d.Employees);

        // Order the resulting names alphabetically
        var orderedEmployees = allEmployees.OrderBy(name => name);

        // Return the result as a List<string>
        return orderedEmployees.ToList();
    }
}