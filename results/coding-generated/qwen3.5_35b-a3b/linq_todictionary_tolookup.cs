public record Student(string Name, string Grade);

public static class GradeBook
        {
            public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
            {
                return students.ToDictionary(s => s.Name);
            }

            public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
            {
                return students.ToLookup(s => s.Grade);
            }
        }