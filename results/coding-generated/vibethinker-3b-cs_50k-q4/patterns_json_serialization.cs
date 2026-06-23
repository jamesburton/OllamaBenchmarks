using System.Text.Json;
using System.Text.Json.Serialize;

public enum Status { Active, Inactive, Pending }
{
    Active,
    Inactive,
    Pending
} : JsonConverter<JsonStringEnumConverter>();

public class PersonDto
{
    [JsonPropertyName("full_name")]
    public string FullName { get; set; }

    [JsonPropertyName("age")]
    public int Age { get; set; }

    [ ignoring("InternalId") ]
    public string InternalId { get; set; }

    [JsonPropertyName("status")]
    public Status Status { get; set; }
}