# OneOf v3.0.271 — Quick Reference

**Package:** `OneOf`
**Namespace:** `using OneOf;`

## Define Discriminated Union

```csharp
// Up to 9 type params
OneOf<User, NotFound, Error> result;

// Custom types (recommended pattern)
[GenerateOneOf]
public partial class GetUserResult : OneOf<User, NotFound, ValidationError> { }
```

## Construct

```csharp
OneOf<User, NotFound> result = new User { Id = 1 };   // implicit
OneOf<User, NotFound> result = OneOf<User, NotFound>.FromT0(user);
OneOf<User, NotFound> result = OneOf<User, NotFound>.FromT1(new NotFound());
```

## Match — exhaustive, returns value

```csharp
string display = result.Match(
    user => user.Name,
    notFound => "Not found"
);
```

## Switch — void, side effects

```csharp
result.Switch(
    user => Console.WriteLine(user.Name),
    notFound => Console.WriteLine("Missing")
);
```

## ASP.NET Controller Pattern

```csharp
public IActionResult GetUser(int id)
{
    OneOf<User, NotFound, ValidationError> result = _service.GetUser(id);
    return result.Match<IActionResult>(
        user    => Ok(user),
        _       => NotFound(),
        err     => BadRequest(err.Message)
    );
}
```

## Type Check / Value Access

```csharp
if (result.IsT0) { User u = result.AsT0; }
result.TryPickT0(out var user, out var remainder); // remainder = OneOf<NotFound,Error>
```

**Source:** https://github.com/mcintyre321/OneOf
