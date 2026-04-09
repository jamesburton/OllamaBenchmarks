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

public record Student(System.String Name, System.String Grade);

public record Student(System.String Name, System.String Grade);

        public static class GradeBook
        {
            public static System.Collections.Generic.Dictionary<System.String, Student> IndexByName(System.Collections.IEnumerable<Student>)
            {
                return students.ToDictionary(s => s.Name); // Wait, ToDictionary is an extension method. I need to call it on the instance.
            }
        }

public static System.Collections.Generic.Dictionary<System.String, Student> IndexByName(System.Collections.IEnumerable<Student> students)
        {
            return students.System.Linq.Enumerable.ToDictionary(s => s.Name); // This syntax is wrong for extension methods usually.
        }

public record Student(System.String Name, System.String Grade);

    public static class GradeBook
    {
        public static System.Collections.Generic.Dictionary<System.String, Student> IndexByName(System.Collections.IEnumerable<Student> students)
        {
            return students.System.Linq.Enumerable.ToDictionary(s => s.Name);
        }

        public static System.Linq.ILookup<System.String, Student> GroupByGrade(System.Collections.IEnumerable<Student> students)
        {
            return students.System.Linq.Enumerable.ToLookup(s => s.Grade);
        }
    }

public record Student(System.String Name, System.String Grade);