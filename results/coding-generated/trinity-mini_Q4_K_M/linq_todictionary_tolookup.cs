public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
        => students.ToDictionary(s => s.Name);

    public static ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
        => students.ToLookup(s => s.Grade);
}/*
Generate the following in a single C# file:

1. A positional record Student with two string properties: Name and Grade.
   Example grades are "A", "B", "C".
   Declare it as: public record Student(string Name, string Grade);

2. A static class GradeBook with two static methods:

   a. Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
      - Uses LINQ ToDictionary with the student's Name as the key.
      - If two students share the same Name, the method should throw (ToDictionary
        throws ArgumentException on duplicate keys — rely on that default behaviour).

   b. ILookup<string, Student> GroupByGrade(IEnumerable<Student> students)
      - Uses LINQ ToLookup with the student's Grade as the key.

Do NOT wrap code in a namespace. Do NOT add using statements.

Return only valid C# code in a single file.
*/
public record Student(string Name, string Grade);

static class GradeBook
{
    public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)