public record Student(string Name, string Grade);

public static class GradeBook
        {
            // Method 1
            public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
            {
                return students.ToDictionary(s => s.Name);
            }

            // Method 2
            public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
            {
                return students.ToLookup(s => s.Grade);
            }
        }