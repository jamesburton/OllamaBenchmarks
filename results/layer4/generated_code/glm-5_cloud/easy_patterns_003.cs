public static class Classifier
{
    public static string ClassifyNumber(int n) =>
        n switch
        {
            < 0 => "negative",
            0 => "zero",
            >= 1 and <= 9 => "small",
            _ => "large"
        };
}