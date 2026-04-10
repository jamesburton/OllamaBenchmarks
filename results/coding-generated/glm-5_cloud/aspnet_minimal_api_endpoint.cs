using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name, decimal Price);
public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _counter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        // Validation
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add("Name", new[] { "Name is required." }); // Or similar message
        }

        if (request.Price <= 0)
        {
            errors.Add("Price", new[] { "Price must be greater than 0." }); // Or similar message
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        // Success
        // Note: Interlocked.Increment is thread-safe, but the prompt implies a simple static counter.
        // "increments a private static int counter". Standard increment `++` is fine for a simple example unless thread safety is strictly required by context.
        // Given the context of a web API, thread safety is usually preferred, but the prompt asks for a "private static int counter... incremented".
        // I'll use `Interlocked.Increment` to be safe, or just `++`. The prompt says "increments a private static int counter".
        // Let's stick to the simplest interpretation of the prompt's text: `_counter++`.
        // However, `Results.Ok` needs the ID.
        // The prompt says: "increments... to generate an Id".
        // So: int id = ++_counter; (or _counter++ then use it).
        // Let's do: int id = ++_counter; (starts at 0, first ID is 1).
        // Wait, "starting at 0". If I do `++_counter`, the first ID is 1. If I do `id = _counter++`, the first ID is 0.
        // Prompt: "increments a private static int counter (starting at 0) to generate an Id".
        // Usually IDs start at 1. But if the counter starts at 0, and we increment it to generate the ID...
        // If I use `Interlocked.Increment(ref _counter)`, it returns the incremented value.
        // Let's assume standard practice: ID starts at 1.
        // But strictly reading: "counter (starting at 0)".
        // If I do `_counter++`, it returns 0 (current) then increments to 1. So ID would be 0.
        // If I do `++_counter`, it increments to 1 then returns 1. So ID would be 1.
        // Let's assume the ID should be unique and sequential.
        // Let's use `Interlocked.Increment` for correctness in a web environment, or just `++` if keeping it simple.
        // Let's stick to `++_counter` logic so the first ID is 1.
        // Actually, let's look at the wording: "increments a private static int counter (starting at 0) to generate an Id".
        // This implies the field is initialized to 0.
        // Logic:
        // _counter++;
        // return Results.Ok(new ProductResponse(_counter, request.Name, request.Price));
        // This results in ID 1 for the first product. This seems the most reasonable interpretation.

        // Let's refine the validation messages. The prompt gives an example: `["Name"] = ["Name is required."]`.
        // I will use similar clear messages.

        // Thread safety: The prompt doesn't explicitly ask for thread safety, but `Results` and ASP.NET Core imply concurrency.
        // However, for a code generation task, simple is often better unless specified.
        // I will use `Interlocked.Increment` just to be professional, or just `++` if I want to match the "simple static field" description literally.
        // The prompt says "increments a private static int counter".
        // I'll use `Interlocked.Increment` to ensure the code is robust, as it's a standard pattern for static counters in web apps.
        // Actually, looking at the "Minimal API" style, simple is key. I'll use `++_counter`.
        // Wait, if I use `++_counter`, the first ID is 1.
        // If I use `_counter++`, the first ID is 0.
        // "starting at 0" usually refers to the initial value of the variable.
        // I will use `++_counter` to get ID 1.

        // Let's write the validation block.
        // The prompt asks to return `Results.ValidationProblem` specifically.
        // It takes `Dictionary<string, string[]>`.

        // Code structure:
        // Check Name.
        // Check Price.
        // If errors, return ValidationProblem.
        // Else, increment counter, return Ok.
    }
}

using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name, decimal Price);
public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _counter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["Name"] = new[] { "Name is required." };
        }

        if (request.Price <= 0)
        {
            errors["Price"] = new[] { "Price must be greater than 0." };
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        // Increment counter to generate Id
        // Using Interlocked for thread safety is good practice, but simple increment matches "increments a private static int counter" description well.
        // I'll use simple increment for brevity unless I suspect thread safety is a strict requirement.
        // "increments a private static int counter" -> _counter++
        // If I do int id = _counter++; id is 0.
        // If I do int id = ++_counter; id is 1.
        // "starting at 0" -> field init.
        // I'll use ++_counter so IDs start at 1.
        int id = ++_counter;

        return Results.Ok(new ProductResponse(id, request.Name, request.Price));
    }
}

if (string.IsNullOrWhiteSpace(request.Name)) return Results.ValidationProblem(...);
     if (request.Price <= 0) return Results.ValidationProblem(...);

if (string.IsNullOrWhiteSpace(request.Name))
         return Results.ValidationProblem(new Dictionary<string, string[]> { ["Name"] = ["Name is required."] });

     if (request.Price <= 0)
         return Results.ValidationProblem(new Dictionary<string, string[]> { ["Price"] = ["Price must be greater than 0."] });

using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name, decimal Price);
public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _counter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> 
            { 
                ["Name"] = new[] { "Name is required." } 
            });
        }

        if (request.Price <= 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> 
            { 
                ["Price"] = new[] { "Price must be greater than 0." } 
            });
        }

        int id = ++_counter;
        return Results.Ok(new ProductResponse(id, request.Name, request.Price));
    }
}