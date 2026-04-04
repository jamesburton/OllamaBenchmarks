public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }

public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)

    {
        var validationContext = new ValidationContext(request);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(request, validationContext, results, validateAllProperties: true))
        {
            var firstError = results.FirstOrDefault();
            if (firstError != null)
            {
                throw new ArgumentException(firstError.ErrorMessage, firstError.MemberName);
            }
            throw new ArgumentException("Validation failed", "");
        }

        return request;
    }