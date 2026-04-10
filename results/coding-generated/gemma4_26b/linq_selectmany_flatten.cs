public record Department(string Name, System.Collections.Generic.List<string> Employees);

    public static class StaffDirectory
    {
        public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.Generic.IEnumerable<Department> departments)
        {
            return System.Linq.Enumerable.OrderBy(
                System.Linq.Enumerable.SelectMany(departments, d => d.Employees),
                name => name
            ).System.Collections.Generic.List<string>(...); // Wait, OrderBy returns IOrderedEnumerable.
        }
    }

public static class StaffDirectory
    {
        public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.Generic.IEnumerable<Department> departments)
        {
            return new System.Collections.Generic.List<string>(
                System.Linq.Enumerable.OrderBy(
                    System.Linq.Enumerable.SelectMany(departments, d => d.Employees),
                    name => name
                )
            );
        }
    }

public record Department(string Name, System.Collections.Generic.List<string> Employees);

    public static class StaffDirectory
    {
        public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.Generic.IEnumerable<Department> departments)
        {
            return new System.Collections.Generic.List<string>(
                System.Linq.Enumerable.OrderBy(
                    System.Linq.Enumerable.SelectMany(departments, d => d.Employees),
                    name => name
                )
            );
        }
    }

public record Department(string Name, System.Collections.Generic.List<string> Employees);

    public static class StaffDirectory
    {
        public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.Generic.IEnumerable<Department> departments)
        {
            return new System.Collections.Generic.List<string>(
                System.Linq.Enumerable.OrderBy(
                    System.Linq.Enumerable.SelectMany(departments, d => d.Employees),
                    name => name
                )
            );
        }
    }

public record Department(string Name, System.Collections.Generic.List<string> Employees);

    public static class StaffDirectory
    {
        public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.Generic.IEnumerable<Department> departments)
        {
            return new System.Collections.Generic.List<string>(
                System.Linq.Enumerable.OrderBy(
                    System.Linq.Enumerable.SelectMany(departments, d => d.Employees),
                    name => name
                )
            );
        }
    }

public record Department(string Name, System.Collections.Generic.List<string> Employees);

    public static class StaffDirectory
    {
        public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.Generic.IEnumerable<Department> departments)
        {
            return new System.Collections.Generic.List<string>(
                System.Linq.Enumerable.OrderBy(
                    System.Linq.Enumerable.SelectMany(departments, d => d.Employees),
                    name => name
                )
            );
        }
    }