public record Address(string Street, string City, string PostalCode);

public static class AddressFormatter
{
    public static string Format(Address address) =>
        $"{address.Street}, {address.City} {address.PostalCode}";
}