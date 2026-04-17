public static class Switcher
{
    public static int ToScore(string grade) => grade switch
    {
        "A" => 4,
        "B" => 3,
        "C" => 2,
        "D" => 1,
        _ => 0
    };
}