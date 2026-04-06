public interface IDataSource
    {
        Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);
    }

int batchNumber = 0;
        while (true)
        {
            if (ct.IsCancellationRequested)
            {
                yield break;
            }

            var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);

            if (batch.Count == 0)
            {
                break;
            }

            foreach (var item in batch)
            {
                yield return item;
            }

            batchNumber++;
        }

using System.Runtime.CompilerServices;

    public interface IDataSource
    {
        Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);
    }

    public class DataProcessor
    {
        public async IAsyncEnumerable<string> ProcessAllBatchesAsync(
            IDataSource source,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            int batchNumber = 0;

            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    yield break;
                }

                var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);

                if (batch.Count == 0)
                {
                    break;
                }

                foreach (var item in batch)
                {
                    yield return item;
                }

                batchNumber++;
            }
        }
    }

. However, as an AI model, I usually output markdown. I will check the system instructions. Usually, providing the code block is safer for readability, but strict adherence might mean raw text.
    Re-reading: "Return only valid C# code in a single file."
    I will output the code directly. Wait, if I output raw text, it might be rendered poorly. I will use a markdown block but ensure the content is exactly what is requested.
    Actually, looking at similar tasks, usually, the user wants the content. I will wrap it in markdown for clarity as is standard for code generation, but ensure no extra text surrounds it.
    Wait, "Return only valid C# code". I should probably not include markdown fences if I want to be strictly compliant with "only valid C# code". However, without fences, it might look like plain text. I'll use fences because it's the standard way to present code in this interface, but I will ensure no explanatory text is outside the fences.
    Actually, looking at the instruction "Return only valid C# code in a single file.", I will provide the code block.

    Wait, one more check on `using`. "Include only the using statement for System.Runtime.CompilerServices (for EnumeratorCancellation). Do NOT add other using statements".
    So `using System.Runtime.CompilerServices;` is required.
    `Task`, `IReadOnlyList`, `CancellationToken`, `IAsyncEnumerable` are in `System.Threading.Tasks` and `System.Collections.Generic`.
    Since the prompt says "the project already has global usings for Xunit, AwesomeAssertions, and NSubstitute", it implies there are global usings for `System` types usually (like `System`, `System.Threading.Tasks`, etc.) in the context of the project.
    However, I must include `using System.Runtime.CompilerServices;`.
    Do I need `using System;` or `using System.Threading.Tasks;`?
    The prompt says: "Do NOT add other using statements — the project already has global usings for Xunit, AwesomeAssertions, and NSubstitute."
    It does *not* explicitly say global usings for `System` exist, but it says "the project already has global usings for...". Usually, in C# projects, `System` is global. But to be safe and strictly follow "Do NOT add other using statements", I should rely on the global usings for `System` types.
    Wait, if I don't add `using System;`, will `Task` and `IReadOnlyList` resolve?
    If the project has global usings, they likely include `System`.
    The prompt says "Include only the using statement for System.Runtime.CompilerServices". This implies I should *not* add `using System;` or `using System.Threading.Tasks;`. I must rely on the existing global usings for those types.
    So I will only write `using System.Runtime.CompilerServices;`.

    Wait, `IReadOnlyList<string>` is in `System.Collections.Generic`. `Task` is in `System.Threading.Tasks`. `CancellationToken` is in `System.Threading`. `IAsyncEnumerable` is in `System.Collections.Generic`.
    If the project has global usings for `System`, `System.Collections.Generic`, `System.Threading.Tasks`, `System.Threading`, then it works.
    Given the constraint "Do NOT add other using statements", I will strictly follow that.

    One check on the loop logic:
    "Checks ct.IsCancellationRequested between batches (yield break if cancelled — do NOT throw)"
    "The loop structure should be: 1. Check cancellation → yield break if cancelled (do NOT throw) 2. Fetch batch 3. Break if empty 4. Yield items"
    My draft: