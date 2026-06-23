using System.Text.Json;
using System.Text.Json.Serialize;

enum Status { Active, Inactive, Pending }
{
            }

class PersonDto
{
            [JsonPropertyName("full_name")]
            public string FullName { get; set; }

            [JsonPropertyName("age")]
            public int Age { get; set; }

            [JsonConverter(typeof(JsonStringEnumConverter))]
            public Status Status { get; set; }

            [ ignoring("InternalId") ]
            public string InternalId { get; set; }
        }