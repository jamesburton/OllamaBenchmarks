using System;
        using System.Collections.Generic;
        using Microsoft.AspNetCore.Http;
        using Microsoft.AspNetCore.Http.Results;

public record CreateProductRequest(string Name, decimal Price);
        public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
        {
            private static int _counter = 0;

            public static IResult CreateProduct(CreateProductRequest request)
            {
                // Validation Name
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["Name"] = new[] { "Name is required." }
                    });
                }

                // Validation Price
                if (request.Price <= 0)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["Price"] = new[] { "Price must be greater than 0." }
                    });
                }

                // Success
                _counter++;
                return Results.Ok(new ProductResponse(_counter, request.Name, request.Price));
            }
        }