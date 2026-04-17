public enum Season { Spring, Summer, Autumn, Winter }

public static class SeasonHelper
{
    public static string GetDescription(Season season) => season switch
    {
        Season.Spring => "The season of new beginnings and blooming flowers.",
        Season.Summer => "The warmest season, perfect for outdoor activities.",
        Season.Autumn => "The season of harvest and falling leaves.",
        Season.Winter => "The coldest season, often associated with snow.",
        _ => "Unknown season."
    };
}