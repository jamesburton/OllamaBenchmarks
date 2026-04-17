public record Student(string Name, double Gpa);

public static class GradeHelper
{
    public static string GetLetterGrade(Student student) => student.Gpa switch
    {
        >= 3.7 => "A",
        >= 3.3 => "B+",
        >= 3.0 => "B",
        >= 2.7 => "C+",
        _ => "C or below"
    };
}