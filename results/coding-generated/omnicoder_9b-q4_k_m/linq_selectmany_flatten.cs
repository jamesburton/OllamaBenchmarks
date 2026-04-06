public record Department(string Name, List<string> Employees);

public static class StaffDirectory
        {
            // Method implementation
        }

public static List<string> GetAllEmployees(IEnumerable<Department> departments)
            {
                return departments
                    .SelectMany(d => d.Employees)
                    .OrderBy(e => e)
                    .ToList();
            }

public record Department(string Name, List<string> Employees);

    public static class StaffDirectory
    {
        public static List<string> GetAllEmployees(IEnumerable<Department> departments)
        {
            return departments
                .SelectMany(d => d.Employees)
                .OrderBy(e => e)
                .ToList();
        }
    }