using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Department { get; set; }
    public decimal Salary { get; set; }
}

public class HrContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("HrDatabase");
    }
}

public static class EmployeeRepository
{
    public static Dictionary<string, decimal> GetAverageSalaryByDepartment(HrContext context)
    {
        return context.Employees
            .GroupBy(e => e.Department)
            .Select(g => new
            {
                Department = g.Key,
                AverageSalary = g.Average(e => e.Salary)
            })
            .ToDictionary(x => x.Department, x => x.AverageSalary);
    }
}