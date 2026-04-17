using System;
using System.Collections.Generic;
using System.Linq;

public record Course(string Id, string Title, int MaxEnrollment);
public record Enrollment(string StudentId, string CourseId);

public class EnrollmentService
{
    private readonly Dictionary<string, Course> _courses = new();
    private readonly List<Enrollment> _enrollments = new();

    public void AddCourse(Course course)
    {
        if (course == null) throw new ArgumentNullException(nameof(course));
        _courses[course.Id] = course;
    }

    public bool Enroll(string studentId, string courseId)
    {
        if (!_courses.TryGetValue(courseId, out var course))
        {
            return false;
        }

        bool alreadyEnrolled = _enrollments.Any(e => e.StudentId == studentId && e.CourseId == courseId);
        if (alreadyEnrolled)
        {
            return false;
        }

        int currentCount = _enrollments.Count(e => e.CourseId == courseId);
        if (currentCount >= course.MaxEnrollment)
        {
            return false;
        }

        _enrollments.Add(new Enrollment(studentId, courseId));
        return true;
    }

    public int EnrollmentCount(string courseId)
    {
        return _enrollments.Count(e => e.CourseId == courseId);
    }
}