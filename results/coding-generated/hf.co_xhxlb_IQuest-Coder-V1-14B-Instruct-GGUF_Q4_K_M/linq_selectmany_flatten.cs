using System;
using System.Collections.Generic;
using System.Linq;

public record Department(string Name, List<string> Employees);

public static class StaffDirectory
{
    public static List<string> GetAllEmployees(IEnumerable<Department> departments)
    {
        return departments
            .SelectMany(department => department.Employees)
            .OrderBy(employee => employee)
            .ToList();
    }
}