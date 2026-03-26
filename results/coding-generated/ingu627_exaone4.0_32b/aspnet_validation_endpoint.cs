using System.ComponentModel.DataAnnotations;

   public class CreateOrderRequest { ... }

   public class OrderService
   {
        public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
        {
            bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, new System.ComponentModel.DataAnnotations.ValidationContext(), true);
            if (!isValid)
            {
                // Get the first error
                var errors = request.GetValidationErrors(); // But how? We don't have a direct way? 
                // Actually, we can use the ValidationContext to collect errors? But we didn't save it.

                // Alternatively, we can get the first error from the errors collection? 
                // We can use: 
                //   var errors = new List<ValidationError>();
                //   Validator.TryValidateObject(request, new ValidationContext(request), errors);
                // But the problem says to use `Validator.TryValidateObject` with `validateAllProperties: true` and then if fails, throw with the first error.

                // How about we do:

                var errors = new List<ValidationError>();
                var context = new ValidationContext(request) { DisplayName = request.Name }; // DisplayName might be needed? 
                bool isValid2 = Validator.TryValidateObject(request, context, errors, true);
                // But the problem says to use the one without the errors parameter? 

                // Actually, the signature we are to use is:
                //   Validator.TryValidateObject(object instance, ValidationContext validationContext, List<ValidationError> errors, bool validateAllProperties)
                // But the problem says: "Validates the request using Validator.TryValidateObject with validateAllProperties: true"

                // However, the example in the problem does not specify the version. We are allowed to use the one that returns a bool and takes a ValidationContext and a list of errors? 

                // But note: the problem says "with validateAllProperties: true", so we must set that.

                // Alternatively, we can use the overload that doesn't take a list of errors? That one returns a bool and doesn't give us the errors. Then we cannot get the first error.

                // Therefore, we must use the overload that takes a list of errors.

                // But the problem says: "using Validator.TryValidateObject" with the parameter `validateAllProperties: true`. It doesn't specify the exact overload.

                // So we can do:

                var errorsList = new List<ValidationError>();
                var context = new ValidationContext(request);
                bool isValid = Validator.TryValidateObject(request, context, errorsList, true);

                if (!isValid)
                {
                    // Get the first error
                    if (errorsList.Count > 0)
                    {
                        var firstError = errorsList[0];
                        string message = $"Validation error: {firstError.Member} - {firstError.ErrorMessage}";
                        throw new System.ArgumentException(message);
                    }
                    else
                    {
                        throw new System.ArgumentException("Validation failed.");
                    }
                }

                return request;
            }
            else
            {
                return request;
            }
        }
   }

 However, note: the problem says "throws an ArgumentException with details of the first validation error". We are constructing a message from the first error.

 But wait: the problem says "the first validation error". We are taking the first element in the list. The list is ordered? According to the documentation, the order is not specified? 

 But in practice, it's usually by property name? We don't care, we just take the first.

 Alternatively, we can use the `ValidationContext` to get the errors? But we are already collecting them.

 But note: the problem does not specify the order, so we take the first in the list.

 However, the problem says "first", meaning the first one encountered? We are not guaranteed, but we'll take the first in the list.

 But note: the problem does not specify how to get the first error. We are using the list and taking the first.

 Also, note: we are creating a new ValidationContext for each validation.

 But the problem does not specify the DisplayName. We set it to the request.Name? Actually, we don't set it. We can leave it unset? 

 We'll create the ValidationContext without setting DisplayName.

 However, the ValidationContext has a DisplayName property that is used in the error message. We can set it to the name of the parameter? But the problem doesn't specify.

 We can set it to "order" or something? But the problem doesn't require a specific message.

 We are just required to throw an ArgumentException with details of the first error.

 We are including the member and the error message.

 But note: the problem says "details of the first validation error". We are including the member (property) and the error message.

 This should be acceptable.

 However, we are using `List<ValidationError>` which requires `using System.Collections.Generic;`? But we are not allowed to have any other using.

 Therefore, we must avoid that.

 How else can we get the first error? 

 There is an alternative: we can use the `Validator.GetValidationErrors` method? But that returns a dictionary? 

 Actually, there is an extension method: `Validator.GetValidationErrors` that returns `IDictionary<string, IEnumerable<ValidationError>>`. 

 But we are not allowed to use that because it requires `using System.Collections.Generic;`? 

 Alternatively, we can use the non-generic `System.Collections.IDictionary`? But that is not available without a using? And we cannot use it without.

 We are stuck? 

 Let me check: the problem says "Only include `using System.ComponentModel.DataAnnotations;` at the top." meaning we cannot have any other using. 

 Therefore, we must avoid any other namespaces.

 How about we use the `ValidationContext` to get the errors without a list? 

 Actually, the `ValidationContext` has a property `Errors` which is a `ValidationErrorCollection`. We can use that? 

 We can do:

   var context = new ValidationContext(request);
   var errors = context.Errors; // This is a ValidationErrorCollection

   bool isValid = Validator.TryValidateObject(request, context, true);

   if (!isValid)
   {
        // Get the first error from the collection
        if (context.Errors.Count > 0)
        {
            var firstError = context.Errors[0]; // This is a ValidationError
            string message = $"Validation error: {firstError.Member} - {firstError.ErrorMessage}";
            throw new System.ArgumentException(message);
        }
        else
        {
            throw new System.ArgumentException("Validation failed.");
        }
   }

 But note: the `ValidationErrorCollection` is in `System.ComponentModel.DataAnnotations` and we are already including that.

 And we don't need any other using.

 Also, the `ValidationContext` is in the same namespace.

 So we can do:

   using System.ComponentModel.DataAnnotations;

   public class CreateOrderRequest { ... }

   public class OrderService
   {
        public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
        {
            var context = new ValidationContext(request);
            bool isValid = Validator.TryValidateObject(request, context, true);

            if (!isValid)
            {
                if (context.Errors.Count > 0)
                {
                    var firstError = context.Errors[0];
                    string message = $"Validation error: {firstError.Member} - {firstError.ErrorMessage}";
                    throw new System.ArgumentException(message);
                }
                else
                {
                    throw new System.ArgumentException("Validation failed.");
                }
            }

            return request;
        }
   }

 This meets the requirement.

 But note: the `ValidationContext` constructor takes an object and an optional DisplayName. We are not setting DisplayName. That's okay.

 Also, note: the `Validator.TryValidateObject` overload we are using is:

   public static bool TryValidateObject(object instance, ValidationContext validationContext, bool validateAllProperties);

 This is available in .NET 10? Yes, because the reference says built-in validation.

 Therefore, we'll use this approach.

 Now, let's write the CreateOrderRequest:

   public class CreateOrderRequest
   {
        [Required]
        public string Name { get; set; }

        [Range(0.01m, 10000m)]
        public decimal Price { get; set; }

        [Range(1, 999)]
        public int Quantity { get; set; }
   }

 Note: the Range for decimal: we use 0.01m and 10000m to specify decimal literals.

 But note: the problem says [Range(0.01, 10000)] for decimal. We can write 0.01m and 10000m to be explicit.

 However, the problem does not specify the suffix. But without the suffix, it would be double? And that might cause a warning? 

 We are using decimal, so we should use decimal literals.

 Alternatively, we can write 0.01m and 10000m.

 But the problem says: [Range(0.01, 10000)] -> without m. But that is acceptable? The compiler will infer? 

 Actually, the Range attribute for decimal expects decimal. If we write 0.01 without m, it is a double and might cause a warning. 

 So we should use 0.01m and 10000m.

 But the problem says: [Range(0.01, 10000)] -> we are to generate exactly that? 

 The problem says: "Generate the following in a single C# file", and the example uses [Range(0.01, 10000)]. 

 However, without the m, it is a double and might not be accepted for a decimal? 

 We have two options:
   a) Generate exactly as the problem says: [Range(0.01, 10000)] -> but that would be a double and cause a warning? 
   b) Use the decimal literals.

 The problem says: "with these validated properties", meaning the attributes as specified. But the specification includes the numbers without suffix.

 However, the problem also says: "It must support object-initializer syntax", which doesn't care about the attribute.

 But to avoid warnings and to be correct, we should use decimal literals.

 But note: the problem says "exactly" the attributes? 

 Let me read: "with these validated properties as public get/set: ... [Range(0.01, 10000)] decimal Price"

 So we are to generate [Range(0.01, 10000)] for the decimal? 

 However, in C#, if we write [Range(0.01, 10000)] for a decimal property, the compiler will issue a warning because the literal is double and we are comparing to decimal? 

 We can avoid the warning by using the decimal literal.

 But the problem says "exactly" the attribute? 

 Since the problem says "exactly", we should generate without the m? 

 But then we get a warning. 

 Alternatively, the problem might not care about warnings? 

 However, the problem says "valid C# code". Warning is not an error, so it's valid? 

 But to be safe and clean, we should use the decimal literal.

 But the problem says: "exactly" the attribute? 

 Let me see the example in the API reference: they use [Range(1,100)] for int and [Range(0.01,10000)] for decimal? 

 Actually, in the example they have:

   app.MapPost("/orders", ([Required] string name, [Range(1,100)] int qty) => ...

 So they are using integers without suffix. For decimal, they don't show.

 But the problem says: [Range(0.01, 10000)] for decimal.

 We'll generate it as [Range(0.01, 10000)] and hope that the compiler will accept it? 

 Actually, the Range attribute has a generic form? No, it's not generic. It takes objects. So it will work, but with a warning.

 Alternatively, we can write:

   [Range(0.01m, 10000m)]

 This is valid and without warning.

 But the problem says "exactly" the attribute? 

 The problem says: "with these validated properties as public get/set: ... [Range(0.01, 10000)] decimal Price"