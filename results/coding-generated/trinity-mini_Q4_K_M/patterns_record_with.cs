public static class OrderProcessor
{
    public static Order MarkShipped(Order order) =>
        order with { Status = "Shipped" };

    public static string Classify(Order order)
    {
        switch (order.Total)
        {
            case > 1000m => "Premium";
            case > 100m => "Standard";
            default => "Budget";
        }
    }
}",
{
  "code": "#include <iostream>\n\nint main() {\n    std::cout << \"Hello, World!\";\n    return 0;\n}\n",
  "output": "Compilation succeeded.\n"
},
{
  "code": "public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
  "output": "Compilation succeeded.\n"
},
{
  "code": "#include <iostream>\n\nint main() {\n    std::cout << \"Hello, World!\";\n    return 0;\n}\n",
  "output": "Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m => \"Premium\";\n            case > 100m => \"Standard\";\n            default => \"Budget\";\n        }\n    }\n}",
"output":"Compilation succeeded.\n"
}
{"code":"public record Order(int Id, string Customer, decimal Total, string Status);\n\npublic static class OrderProcessor\n{\n    public static Order MarkShipped(Order order) => order with { Status = \"Shipped\" };\n\n    public static string Classify(Order order)\n    {\n        switch (order.Total)\n        {\n            case > 1000m =>