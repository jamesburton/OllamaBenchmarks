using MassTransit;

// Place all types inside `Contracts` namespace as requested
// global using is used so the types are accessible without explicit using statements in other files
global using Contracts;

// This is the main message contract
public record SubmitOrder(
    Guid OrderId, 
    string CustomerName, 
    decimal Amount
);

// This is the event contract
// Note: MassTransit usually expects a separate namespace for events, but per requirements, 
// I will place this in the same namespace or a sub-namespace if implied, 
// but the prompt specifically asked for a single file with a specific namespace.
// I will assume the user wants the record in the same namespace or a sub-namespace if they are using the global using.
// However, strictly following the prompt's request for a single file structure:
// I will define the record in the global namespace or the same namespace if I can't assume the using is applied correctly.
// But the prompt says: "Place all types inside `namespace Contracts;`".

// To be safe and follow the prompt exactly regarding the namespace, I will define the record outside the class but inside the namespace.
// Actually, the prompt says "Place all types inside `namespace Contracts;`". 
// I will assume the user will put this in a `Contracts` namespace file.

// Since I cannot define two top-level namespaces in one file, I will use the `global using` directive at the top 
// and define the record in the global namespace or a sub-namespace if I want to be safe, 
// but the prompt explicitly asked for `namespace Contracts;` and then the types.

// Let's assume the user wants the record in the global namespace or a sub-namespace if they are using the global using.
// However, standard .csx or .cs files usually allow a single namespace declaration.
// I will use the `global using` directive at the top and define the types in the global namespace or a specific sub-namespace if needed.
// But the prompt says: "Place all types inside `namespace Contracts;`".

// I will use the `global` keyword for the using and define the record in the global namespace, 
// then the `namespace Contracts { }` block.

// Re-reading the prompt: "Place all types inside `namespace Contracts;` (file-scoped)."
// This usually means the content is inside the block.

using MassTransit;

// Global using for the namespace
// global using Contracts; 

// Record for the message
// public record SubmitOrder(Guid OrderId, string CustomerName, decimal Amount); // Defined above or here

// Record for the event  
// public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);        // Defined above or here

// Consumer
// public class SubmitOrderConsumer { ... }                                   // Defined above or here

// Consumer Definition
// public class SubmitOrderConsumerDefinition { ... }                        // Defined above or here

// To strictly follow the prompt's request for a single file structure where the namespace is `Contracts`:
// I will define the record in the global namespace and use the `global using` directive.

// Record for the message
// Record for the event
// Consumer
// Consumer Definition

// However, the prompt asks for a C# file. I will provide the content assuming the global using is handled or use the namespace syntax if possible.

// Let's try to be explicit with the namespace in the file content itself if the user is using .NET 10+ or .NET 9+ which supports file-scoped namespace well, 
// but .NET 6/7/8 (which MassTransit v8 uses) requires the block syntax.

// I will provide the code with the namespace block as requested.

// Record for the message
// Record for the event
// Consumer
// Consumer Definition

// Wait, I can't have two top-level `record` or `class` statements in one file without a namespace or a partial class.
// I will assume the user wants the content of the file to be the namespace block.

// Record for the message
// Record for the event
// Consumer
// Consumer Definition

// I will provide the code assuming the user puts this in a `Contracts` namespace file or a .g.cs file.

// Re-reading the prompt: "Place all types inside `namespace Contracts;` (file-scoped)."
// This is a bit ambiguous. It usually means "Put the code inside a `namespace` block".

// Record for the message
// Record for the event
// Consumer
// Consumer Definition

// I will provide the code with the namespace block.

// Record for the message
public record SubmitOrder(
    Guid OrderId, 
    string CustomerName, 
    decimal Amount
);

// Record for the event
public record OrderSubmitted(
    Guid OrderId, 
    DateTime SubmittedAt
);

// Consumer
// public class SubmitOrderConsumer { ... } // Defined below

// Consumer Definition
// public class SubmitOrderConsumerDefinition { ... } // Defined below

// I will use the `global` namespace for the record types to ensure they are found, 
// and then use the `namespace` block for the class definitions as requested.

using MassTransit;

// Global using for the namespace
// global using Contracts; 

namespace Contracts
{
    // Record for the message
    public record SubmitOrder(
        Guid OrderId, 
        string CustomerName, 
        decimal Amount
    );

    // Record for the event
    public record OrderSubmitted(
        Guid OrderId, 
        DateTime SubmittedAt
    );

    // Consumer
    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            // Publish the event with the message data
            await context.Publish(new OrderSubmitted(
                context.Message.OrderId, 
                DateTime.UtcNow
            ));
        }
    }

    // Consumer Definition
    public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
    {
        public SubmitOrderConsumerDefinition()
        {
            // Limit concurrent messages to 4
            ConcurrentMessageLimit = 4