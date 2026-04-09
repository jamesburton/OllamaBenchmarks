public record Department(string Name, List<string> Employees);

    public static class StaffDirectory
    {
        public static List<string> GetAllEmployees(IEnumerable<Department> departments)
        {
            return departments
                .SelectMany(d => d.Employees)
                .OrderBy(name => name)
                .ToList();
        }
    }

    Wait, I need to make sure I don't add any markdown like