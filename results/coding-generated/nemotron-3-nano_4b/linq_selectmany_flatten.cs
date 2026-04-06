public record Department(string Name, List<string> Employees);

static class StaffDirectory
{
    public static List<string> GetAllEmployees(IEnumerable<Department> departments)
    {
        return departments
            .SelectMany(d => d.Employees)
            .OrderBy(n => n)
            .ToList();
    }
}