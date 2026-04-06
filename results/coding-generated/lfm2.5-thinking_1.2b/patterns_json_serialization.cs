using System.Text.Json;
using System.Text.Json.Serialization;

public enum Status { Active, Inactive, Pending }
public class PersonDto {
    public string FullName { get; set; }
    public int Age { get; set; }
    public string InternalId { get; set; }
    public Status Status { get; set; }
}