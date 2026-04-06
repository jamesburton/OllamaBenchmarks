public record Student(string Name, string Grade);

static class GradeBook {
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students) {
        return students.ToDictionary(student => student.Name);
    }

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students) {
        return students.ToLookup(student => student.Grade);
    }
}