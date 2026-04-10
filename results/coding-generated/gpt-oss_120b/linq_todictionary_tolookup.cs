public record Student(string Name, string Grade);

public static class GradeBook
{
    public static System.Collections.Generic.Dictionary<string, Student> IndexByName(
        System.Collections.Generic.IEnumerable<Student> students)
    {
        // Uses LINQ's ToDictionary; duplicate keys will cause an ArgumentException automatically.
        return System.Linq.Enumerable.ToDictionary(students, s => s.Name);
    }

    public static System.Linq.ILookup<string, Student> GroupByGrade(
        System.Collections.Generic.IEnumerable<Student> students)
    {
        // Uses LINQ's ToLookup to group students by their Grade.
        return System.Linq.Enumerable.ToLookup(students, s => s.Grade);
    }
}