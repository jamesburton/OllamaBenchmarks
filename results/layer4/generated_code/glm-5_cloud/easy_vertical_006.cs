using System;
using System.Collections.Generic;
using System.Linq;

public record Employee(int Id, string Name, string Department, decimal Salary);

public interface IEmployeeRepository
{
    void Add(Employee employee);
    List<Employee> GetByDepartment(string department);
    List<Employee> GetAll();
}

public class InMemoryEmployeeRepository : IEmployeeRepository
{
    private readonly List<Employee> _employees = new();

    public void Add(Employee employee)
    {
        _employees.Add(employee);
    }

    public List<Employee> GetByDepartment(string department)
    {
        return _employees.Where(e => e.Department == department).ToList();
    }

    public List<Employee> GetAll()
    {
        return _employees.ToList();
    }
}