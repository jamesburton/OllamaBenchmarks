if (string.IsNullOrWhiteSpace(request.Name)) {
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["Name"] = new[] { "Name is required." } });
        }
        if (request.Price <= 0) {
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["Price"] = new[] { "Price must be greater than 0." } });
        }
        _idCounter++;
        return Results.Ok(new ProductResponse(_idCounter, request.Name, request.Price));