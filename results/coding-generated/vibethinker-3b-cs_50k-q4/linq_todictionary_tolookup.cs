public static Dictionary<string, Student> IndexByName(IEnumerable<Student> students)
{
    return ToDictionary(s => s.Name);
}