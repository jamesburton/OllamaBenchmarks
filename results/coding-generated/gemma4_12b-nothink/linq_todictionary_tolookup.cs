public record Student(string Name, string Grade);

public static class GradeBook
{
    public static System.Collections.Generic.Dictionary<string, Student> IndexByName(System.Collections.Generic.IEnumerable<Student> students)
    {
        return students.ToDictionary(s => s.Name);
    }

    public static System.Linq.ILookup<string, Student> GroupByGrade(System.Collections.Generic.IEnumerable<Student> students)
    {
        return students.ToLookup(s => s.Grade);
    }
}