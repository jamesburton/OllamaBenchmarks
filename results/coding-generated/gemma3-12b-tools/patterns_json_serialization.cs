using System.Text.Json;
using System.Text.Json.Serialization;

public enum Status
{
    [JsonName("active")]
    Active,
    [JsonName("inactive")]
    Inactive,
    [JsonName("pending")]
    Pending
}

public class PersonDto
{
    [JsonPropertyName("full_name")]
    public string FullName { get; set; }

    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonIgnore]
    public string InternalId { get; set; }

    [JsonPropertyName("status")]
    public Status Status { get; set; }
}