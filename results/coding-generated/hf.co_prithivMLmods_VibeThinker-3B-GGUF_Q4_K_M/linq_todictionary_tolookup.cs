public record Student(string Name, string Grade);

public static class GradeBook
{
    public Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
    {
        return LINQ.ToDictionary(s => s.Name);
    }

    public ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
    {
        return LINQ.ToLookup(s => s.Grade);
    }
}