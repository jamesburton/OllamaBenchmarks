using System;
using System.Collections.Generic;
using System.Linq;

public record ExamResult(string StudentId, string Subject, int Score);

public class GradeBook
{
    private readonly List<ExamResult> _results = new List<ExamResult>();

    public void Record(ExamResult result)
    {
        _results.Add(result);
    }

    public double AverageScore(string studentId)
    {
        var studentResults = _results.Where(r => r.StudentId == studentId).ToList();

        if (studentResults.Count == 0)
        {
            return 0;
        }

        return studentResults.Average(r => r.Score);
    }

    public string Grade(string studentId)
    {
        double average = AverageScore(studentId);

        if (average >= 90) return "A";
        if (average >= 80) return "B";
        if (average >= 70) return "C";
        if (average >= 60) return "D";
        return "F";
    }

    public List<ExamResult> GetResultsForStudent(string studentId)
    {
        return _results.Where(r => r.StudentId == studentId).ToList();
    }
}