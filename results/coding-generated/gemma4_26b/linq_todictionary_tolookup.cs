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