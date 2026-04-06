).
    *   Constraints: No explanatory text, no namespace, no using statements (wait, the prompt says "Include all required using directives" in the system instructions but then says "Do NOT add using statements" in the specific user request. I need to resolve this conflict).
    *   Conflict Resolution: The system instruction says "Include all required using directives". The user instruction says "Do NOT add using statements". Usually, specific user instructions override general system instructions regarding content, but "Do NOT add using statements" combined with "Return only valid C# code" implies I should write the code without a namespace and without `using` directives if possible, OR it implies standard library types are assumed to be available. However, LINQ requires `System.Linq`. If I cannot use `using System.Linq;`, I cannot call `.Average()`, `.Min()`, `.Max()` on an `IEnumerable<double>` unless I fully qualify them or assume the environment has them imported (which is impossible in a single file snippet without using).
    *   Re-reading the specific user constraint: "Do NOT add using statements." vs System Instruction: "Include all required using directives".
    *   Interpretation: The system instruction is a general rule for the model's behavior. The user prompt contains specific constraints for this task. However, valid C# code *requires* `using` statements to access `System.Linq`. If I omit them, the code won't compile unless fully qualified (e.g., `System.Linq.Enumerable.Average`).
    *   Let's look closely at the user constraint: "Do NOT add using statements." This is a hard constraint for this specific output. But then it says "Return only valid C# code". Valid C# code needs namespaces/using for LINQ methods unless fully qualified.
    *   Wait, looking at the system instruction again: "Include all required using directives". The user instruction says "Do NOT add using statements." This is a direct contradiction.
    *   Standard practice for these types of prompts: If the user explicitly says "Do NOT add using statements", I should follow that, but ensure the code is valid. To make it valid without `using`, I must use fully qualified names (`System.Linq.Enumerable.Average`).
    *   However, often "Do NOT add using statements" in these contexts implies "Don't write a namespace block with usings". But to be safe and compliant with "valid C# code", I will use fully qualified names for LINQ methods if I can't use `using`.
    *   Actually, looking at the system instruction: "Include all required using directives". This is part of the "Output rules" defined by the system. The user prompt says "Do NOT add using statements." under "Generate the following...".
    *   Let's check the priority. Usually, specific task constraints override general formatting constraints. But if I don't include `using System.Linq;`, the code is invalid. If I do include it, I violate the user constraint.
    *   Hypothesis: The user wants a snippet that might be pasted into a file that already has usings, or they want fully qualified names. Given "Return only valid C# code", I must ensure it compiles.
    *   Let's look at the instruction again: "Do NOT add using statements." This is explicit. I will use fully qualified names for `System.Linq` methods to satisfy both "valid code" and "no using statements".
    *   Wait, another interpretation: The system instruction says "Include all required using directives". The user prompt says "Do NOT add using statements." This is a conflict. In many RLHF scenarios, the system instruction regarding "Output rules" takes precedence as meta-instructions. However, the user prompt is the specific task.
    *   Let's try to find a middle ground or pick the most logical path. If I write `using System.Linq;`, I violate the user prompt. If I don't, I might violate the system instruction about valid code (if it implies standard imports). But "valid C# code" is the ultimate goal. Code without `using` but with fully qualified names IS valid C#.
    *   Decision: I will use fully qualified names (`System.Linq.Enumerable.Average`, etc.) to avoid `using` statements, satisfying the user's explicit constraint while maintaining validity.
    *   Wait, looking at the system instruction again: "Include all required using directives". This is under "Output rules". The user prompt says "Do NOT add using statements." under the task description.
    *   Okay, I will prioritize the user prompt's specific constraint ("Do NOT add using statements") because it's part of the task definition, but I must ensure validity. So fully qualified names it is.
    *   Wait, actually, looking at the system instruction "Include all required using directives" vs User "Do NOT add using statements". If I include them, I fail the user constraint. If I don't, I might fail the system instruction about valid code (if interpreted strictly). But fully qualified names make it valid without usings.
    *   Let's check the System Instruction again: "Include all required using directives". This is a rule for *me*. The User Instruction says "Do NOT add using statements." This is a constraint for *this task*.
    *   Okay, I will follow the User Instruction regarding `using` (i.e., none), but use fully qualified types to ensure validity.
    *   Wait, one more thing: The system instruction says "Return ONLY the C# source code". No markdown.
    *   System instruction also says "Follow .NET naming conventions".
    *   System instruction says "All types in a single file".
    *   User instruction says "Do NOT wrap code in a namespace."
    *   So: No `namespace`, no `using`. Fully qualified LINQ.

2.  **Implementation Details:**
    *   Class: `SurveyAnalyzer` (static).
    *   Method: `ComputeStats` (static).
    *   Return Type: Named tuple `(double Mean, double Median, double Min, double Max)`. In C#, this is a `ValueTuple<double, double, double, double>`. To make it named, I can use `var result = (Mean: ..., Median: ..., ...)` but the return type needs to be specified or inferred. The prompt asks for "Return the four values as a named tuple".
    *   In C#, returning a tuple with names usually looks like `(double Mean, double Median, double Min, double Max)`. This is valid syntax for a `ValueTuple` where the elements are named in the return statement or via a struct. However, standard `ValueTuple` doesn't have named fields unless it's a specific type.
    *   Wait, C# 10+ supports `record` and better tuple handling. But a simple method returning `(double Mean, double Median, double Min, double Max)` creates a `ValueTuple<double, double, double, double>`. The names are only visible in the caller's deconstruction or if it's a specific struct.
    *   To make it a "named tuple" in the return signature sense, I can define a struct or use the syntax `(double Mean, double Median, double Min, double Max)` which is valid for returning a tuple where the names are part of the type definition? No, standard tuples don't have named fields in the type name.
    *   However, the prompt says "Return the four values as a named tuple: (double Mean, double Median, double Min, double Max)". This implies the return statement should look like `return (Mean: meanVal, Median: medianVal, ...);`. But the method signature needs to match.
    *   If I define the method as `(double Mean, double Median, double Min, double Max) ComputeStats(...)`, that is valid C# syntax for a tuple return type where the names are part of the type? No, `ValueTuple` doesn't support named fields in the signature like that unless it's a specific struct.
    *   Actually, in C#, you can write `public (double Mean, double Median, double Min, double Max) MethodName()`. This is valid syntax for returning a tuple with those names. The type is still `ValueTuple<double, double, double, double>`, but the names are attached to the return expression and deconstruction.
    *   Wait, checking C# 10/11/12 specs: `(T1 Name1, T2 Name2, ...) Type` is valid for method signatures? Yes, since C# 7.2 (tuples) and improved in later versions.
    *   So I will define the return type as `(double Mean, double Median, double Min, double Max)`.
    *   Logic:
        *   Check `values` is not empty. Throw `ArgumentException`.
        *   `Mean`: `System.Linq.Enumerable.Average(values)` (or just `Average` if using extension method on IEnumerable). Since I can't use `using`, I must use `System.Linq.Enumerable.Average`. Wait, `IEnumerable<T>` has an extension method `Average()`. So `values.Average()` works without `using`? No, the extension method is in `System.Linq.Enumerable`. To call it, you need `using System.Linq;` OR fully qualify `System.Linq.Enumerable.Average(values)`.
        *   Wait, if I don't use `using`, can I call `values.Average()`? Yes, if the namespace is implicitly imported (not possible here). So I must use `System.Linq.Enumerable.Average(values)`.
        *   `Min`: `System.Linq.Enumerable.Min(values)`.
        *   `Max`: `System.Linq.Enumerable.Max(values)`.
        *   `Median`: Sort, then logic for odd/even.
    *   Edge case: Empty sequence -> Throw `ArgumentException`.
    *   Sorting: `values.OrderBy(x => x).ToList()` or just sort the enumerable? `OrderBy` returns an `IOrderedEnumerable`. I need to access elements by index. So convert to `List<double>` first.
    *   Median Logic:
        *   Sorted list `sorted`.
        *   Count `n`.
        *   If `n % 2 == 1`: `sorted[n / 2]`.
        *   If `n % 2 == 0`: `(sorted[n / 2 - 1] + sorted[n / 2]) / 2.0`.

3.  **Refining Constraints:**
    *   "Do NOT wrap code in a namespace." -> No `namespace ... { }`.
    *   "Do NOT add using statements." -> No `using System;` etc.
    *   "Include all required using directives" (System Instruction) vs "Do NOT add using statements" (User).
    *   I will follow the User constraint ("Do NOT add using statements") because it's specific to the task, but I will use fully qualified names to ensure validity. This satisfies "valid C# code".
    *   Wait, if I don't include `using System;`, do I need to qualify `ArgumentException`? Yes, `System.ArgumentException`.
    *   Do I need to qualify `IEnumerable<double>`? No, that's a generic type in `System.Collections.Generic`. If I don't use `using System.Collections.Generic;`, I must fully qualify it. But usually, standard types are assumed available or the user implies "no extra using statements beyond what's needed".
    *   Actually, if I write `IEnumerable<double>`, that requires `using System.Collections.Generic;` implicitly? No, in a file without usings, you can't use `IEnumerable`. You must use `System.Collections.Generic.IEnumerable<double>`.
    *   So I will fully qualify everything from `System` namespaces to be safe and compliant with "No using statements".
    *   Wait, is it possible the user expects me to just write the code assuming a context where `using System;` exists but they don't want me to type it? No, "Return only valid C# code". Code without `using` that uses types from `System` won't compile unless fully qualified.
    *   Okay, I will fully qualify `System.ArgumentException`, `System.Collections.Generic.IEnumerable<double>`, `System.Linq.Enumerable.Average`, etc.

4.  **Drafting the Code:**