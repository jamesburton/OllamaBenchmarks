using System;
using System.Collections.Generic;
using System.Linq;

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