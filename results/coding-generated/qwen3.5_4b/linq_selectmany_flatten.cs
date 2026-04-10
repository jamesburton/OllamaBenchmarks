public record Department(string Name, List<string> Employees);

public static class StaffDirectory
        {
            // Method implementation
        }

public static List<string> GetAllEmployees(IEnumerable<Department> departments)
        {
            return departments
                .SelectMany(d => d.Employees)
                .OrderBy(n => n)
                .ToList();
        }

public record Department(string Name, System.Collections.Generic.List<string> Employees);

    public static class StaffDirectory
    {
        public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.IEnumerable<Department> departments)
        {
            return departments
                .SelectMany(d => d.Employees) // Wait, SelectMany is in System.Linq.Enumerable
                .OrderBy(n => n)
                .ToList();
        }
    }

public record Department(string Name, System.Collections.Generic.List<string> Employees);

    public static class StaffDirectory
    {
        public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.IEnumerable<Department> departments)
        {
            return departments
                .SelectMany(d => d.Employees)
                .OrderBy(n => n)
                .ToList();
        }
    }

public record Department(string Name, System.Collections.Generic.List<string> Employees);

    public static class StaffDirectory
    {
        public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.IEnumerable<Department> departments)
        {
            return departments
                .SelectMany(d => d.Employees)
                .OrderBy(n => n)
                .ToList();
        }
    }