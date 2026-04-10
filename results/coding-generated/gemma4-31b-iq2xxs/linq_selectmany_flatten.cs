public static System.Collections.Generic.List<string> GetAllEmployees(System.Collections.Generic.IEnumerable<Department> departments)
    {
        var flat = System.Linq.Enumerable.SelectMany(departments, d => d.Employees);
        var ordered = System.Linq.Enumerable.OrderBy(flat, name => name);
        return System.Collections.Generic.List<string>.OrderBy(...) // Wait, the return type is List<string>.
        // To get a List, I need to convert the sequence.
    }