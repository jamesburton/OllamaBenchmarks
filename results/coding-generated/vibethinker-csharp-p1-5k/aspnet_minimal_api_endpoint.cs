using Microsoft.AspNetCore.Http;

public static IResult CreateProduct(CreateProductRequest request)
{
            // Validate Name
            if (string.IsNullOrEmpty(request.Name))
            {
                return Results validityProblem(new Dictionary<string, string[]> { ["Name"] = "Name is required." });
            }

            // Validate Price
            if (request.Price <= 0)
            {
                return Results validationProblem(new Dictionary<string, string[]> { ["Price"] = "Price must be greater than zero." });
            }

            // Increment counter to generate Id
            ProductEndpoints.counter++;
            int id = ProductEndpoints.counter;

            return Results Ok(new ProductResponse(id, request.Name, request.Price));
        }