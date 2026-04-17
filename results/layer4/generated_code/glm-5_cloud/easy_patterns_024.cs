using System;

public static class Formatter
{
    public static string FormatCurrency(decimal amount, string currency)
    {
        var symbol = currency switch
        {
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            _ => currency + " "
        };

        return $"{symbol}{amount}";
    }
}