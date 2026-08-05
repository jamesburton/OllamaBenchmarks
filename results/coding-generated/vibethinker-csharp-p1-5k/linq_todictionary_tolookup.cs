public record Student(string Name, string Grade);
public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
{
            return ToDictionary(s => s.Name);
        }
public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
{
            return ToLookup(s => s.Grade);
        }