using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

record GitHubUser(string Login, string Name, int PublicRepos) {
    [System.Text.Json.Serialization.JsonPropertyName("login")]
    public string Login { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("public_repos")]
    public int PublicRepos { get; init; }
}

// Wait, record syntax with properties.
// In C#, records can have properties defined inline or via the constructor if they are not just data classes.
// But for a Record to have properties that need attributes, I should define them explicitly or use the property syntax.
// Actually, `record GitHubUser(string Login, string Name, int PublicRepos)` creates implicit properties.
// To add attributes to properties in a record, I can use `[JsonPropertyName]` on the constructor parameters? No, attributes on properties.
// In C# 10+, records support property accessors.
// `record GitHubUser(string Login, string Name, int PublicRepos)` creates properties named `Login`, `Name`, `PublicRepos`.
// I can add attributes to these properties.
// However, the prompt asks for "Use positional record syntax".
// `record GitHubUser(string Login, string Name, int PublicRepos)` is positional.
// To add attributes to the properties:
// `[System.Text.Json.Serialization.JsonPropertyName("login")]`
// public string Login { get; init; }
// But records don't allow explicit property declarations in the same way classes do unless using `record ... { ... }`.
// Actually, I can define the record with properties explicitly if needed, but standard positional record syntax implies implicit properties.
// To add attributes to implicit properties in a record, I need to use `[JsonPropertyName]` on the constructor parameters? No, that's for data binding sometimes.
// The correct way to attribute properties in a record is usually via the property definition if explicit, or using `record ... { [Attribute] public string Login { get; init; } }`.
// Let's check C# 10 Record syntax.
// `record GitHubUser(string Login, string Name, int PublicRepos)` creates properties.
// To add attributes to them: `[System.Text.Json.Serialization.JsonPropertyName("login")]` on the property declaration.
// So I should write:
// record GitHubUser {
//     [System.Text.Json.Serialization.JsonPropertyName("login")] public string Login { get; init; }
//     ...
// }
// But the prompt says "Use positional record syntax". This usually means `record GitHubUser(string Login, string Name, int PublicRepos)`.
// If I use implicit properties, I cannot easily add attributes to them without changing the signature or using a different approach.
// However, `[JsonPropertyName]` can be applied to constructor parameters in some contexts (like data binding), but for serialization attributes on properties, they need to be on the property itself.
// In C# 10+, you can define records with explicit properties: `record GitHubUser { [Attr] public string Login { get; init; } ... }`. This is not strictly "positional record syntax" (which implies constructor args).
// But "Use positional record syntax" might refer to the initialization style or just the fact that it's a record.
// Actually, `[JsonPropertyName]` on constructor parameters works for serialization in some scenarios, but standard practice is on properties.
// Let's try to combine: `record GitHubUser(string Login, string Name, int PublicRepos)`.
// If I add attributes to the constructor parameters, does it work? Yes, for JSON serialization, `[JsonPropertyName]` on record constructor arguments works in .NET Core 10 (as per prompt context).
// Wait, the prompt says "Use positional record syntax". This implies `record GitHubUser(string Login, string Name, int PublicRepos)`.
// If I add attributes to the parameters: `[System.Text.Json.Serialization.JsonPropertyName("login")]` on `string Login`.
// Let's check if that is valid. Yes, in records, constructor arguments are properties.
// So I will put the attributes on the constructor parameters.

// Wait, looking at the instruction again: "Add [System.Text.Json.Serialization.JsonPropertyName("...")] attributes".
// It implies adding attributes to the record definition.
// I will place them on the constructor parameters as that is the most direct way with positional syntax.

// Wait, one more thing: The prompt says "Use positional record syntax."
// `record GitHubUser(string Login, string Name, int PublicRepos)`
// If I add attributes to the parameters, it looks like this:
// [System.Text.Json.Serialization.JsonPropertyName("login")] public string Login { get; init; } is not valid for positional constructor args directly without explicit property declaration.
// Actually, in C# 10+, you can do:
// record GitHubUser(string Login, string Name, int PublicRepos) { ... }
// But attributes on parameters?
// `[System.Text.Json.Serialization.JsonPropertyName("login")]`
// `public string Login { get; init; }`
// This is explicit property syntax.
// If I use `record GitHubUser(string Login, string Name, int PublicRepos)` without properties block, the properties are implicit.
// Can I add attributes to implicit properties? No.
// So I must define them explicitly or use a different approach.
// However, "Use positional record syntax" usually means defining the record with constructor arguments.
// To satisfy both: I will define the record with explicit properties but initialize them via the constructor (which is still positional in nature regarding data flow).
// Actually, `record GitHubUser(string Login, string Name, int PublicRepos)` creates implicit properties.
// If I want attributes on properties, I need to declare them.
// `record GitHubUser { [Attr] public string Login { get; init; } ... }` is not positional constructor syntax.
// But `record GitHubUser(string Login, string Name, int PublicRepos)` IS positional.
// Can I add attributes to the parameters? Yes. `[JsonPropertyName("login")]` on `string Login`.
// Let's verify if this works for JSON serialization in .NET Core 10.
// Yes, constructor arguments in records are serialized as properties. Attributes on them work.
// So:
// record GitHubUser([System.Text.Json.Serialization.JsonPropertyName("login")] string Login, [System.Text.Json.Serialization.JsonPropertyName("name")] string Name, [System.Text.Json.Serialization.JsonPropertyName("public_repos")] int PublicRepos)

// Wait, the prompt says "Add ... attributes". It doesn't explicitly say "on constructor parameters".
// But given the constraint "Use positional record syntax", putting them