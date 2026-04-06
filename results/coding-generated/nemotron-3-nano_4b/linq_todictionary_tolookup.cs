public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}