using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public string CourseName { get; set; } = string.Empty;
}

public class SchoolContext : DbContext
{
    public DbSet<Student> Students { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("SchoolDatabase");
    }
}

public static class SchoolRepository
{
    public static Student? GetWithEnrollments(SchoolContext context, int studentId)
    {
        return context.Students
            .Include(s => s.Enrollments)
            .FirstOrDefault(s => s.Id == studentId);
    }
}