public static List<string> GetAllEmployees(IEnumerable<Department> departments)
    => departments.SelectMany(d => d.Employees).OrderBy(e => e).ToList();