public record Student(string Name, string Grade);  
public static class GradeBook {  
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students) {  
        var dict = new Dictionary<string, Student>();  
        foreach (var s in students) {  
            dict[s.Name] = s;  
        }  
        return dict;  
    }  
    public static List<List<Student>> GroupByGrade(IEnumerable<Student> students) {  
        var groups = students.GroupBy(s => s.Grade);  
        var result = new List<List<Student>>();  
        foreach (var group in groups) {  
            result.Add(group.ToList());  
        }  
        return result;  
    }  
}