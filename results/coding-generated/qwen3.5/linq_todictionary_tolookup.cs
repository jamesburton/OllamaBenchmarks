public record Student(string Name, string Grade);

public static class GradeBook
        {
            public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
            {
                // Need to use fully qualified names for LINQ and Collections
                return System.Linq.Enumerable.ToDictionary(students, s => s.Name);
            }

            public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
            {
                return System.Linq.Enumerable.ToLookup(students, s => s.Grade);
            }
        }