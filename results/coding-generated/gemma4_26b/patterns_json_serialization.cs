using System.Text.Json;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
    Active,
    Inactive,
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