public record EmailAddress(string Value)
{
    public static EmailAddress? TryParse(string input)
    {
        if (input.Contains('@') && input.Contains('.'))
        {
            return new EmailAddress(input);
        }
        return null;
    }
}