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

public record Student(string Name, string Grade);

        public static class GradeBook
        {
            public static System.Collections.Generic.Dictionary<string, Student> IndexByName(System.Collections.Generic.IEnumerable<Student> students)
            {
                return System.Linq.Enumerable.ToDictionary(students, s => s.Name);
            }

            public static System.Collections.Generic.ILookup<string, Student> GroupByGrade(System.Collections.Generic.IEnumerable<Student> students)
            {
                return System.Linq.Enumerable.ToLookup(students, s => s.Grade);
            }
        }