using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class BenchmarkQuality
{
    private static readonly HttpClient client = new HttpClient { Timeout = TimeSpan.FromMinutes(20) };
    private const string OllamaBaseUrl = "http://127.0.0.1:11434";

    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: dotnet run benchmark_quality.cs --models <model1> <model2> ...");
            return;
        }

        var models = new List<string>();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--models" && i + 1 < args.Length)
            {
                for (int j = i + 1; j < args.Length && !args[j].StartsWith("--"); j++)
                {
                    models.Add(args[j]);
                }
            }
        }

        if (models.Count == 0) { Console.WriteLine("No models specified."); return; }

        foreach (var model in models)
        {
            Console.WriteLine($"--- Benchmarking Quality for {model} ---");
            var result = await RunModel(model);
            Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    private static async Task<Dictionary<string, object>> RunModel(string model)
    {
        var row = new Dictionary<string, object>
        {
            ["model"] = model,
            ["coding_pass"] = 0,
            ["coding_total"] = 2,
            ["tool_pass"] = 0,
            ["tool_total"] = 0, // Simplified for now
            ["agentic_pass"] = 0,
            ["agentic_total"] = 0
        };

        // Longest Common Prefix
        if (await TestCoding(model, "Define longest_common_prefix(strs: list[str]) -> str. Handle empty list.", 
            new[] { "assert longest_common_prefix([\"flower\",\"flow\",\"flight\"])==\"fl\"", "assert longest_common_prefix([])==\"\"" }))
        {
            row["coding_pass"] = (int)row["coding_pass"] + 1;
        }

        // Top K Frequent
        if (await TestCoding(model, "Define top_k_frequent(nums: list[int], k: int) -> list[int]. Sort by frequency desc, tie by smaller number first.", 
            new[] { "assert top_k_frequent([1,1,1,2,2,3],2)==[1,2]" }))
        {
            row["coding_pass"] = (int)row["coding_pass"] + 1;
        }

        row["score"] = (int)row["coding_pass"];
        row["score_max"] = (int)row["coding_total"];
        return row;
    }

    private static async Task<bool> TestCoding(string model, string prompt, string[] asserts)
    {
        try
        {
            var payload = new
            {
                model = model,
                prompt = "Return only Python code. " + prompt,
                stream = false,
                options = new { num_predict = 256, temperature = 0, seed = 42 }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{OllamaBaseUrl}/api/generate", content);
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement.GetProperty("response").GetString();

            var code = ExtractCode(text);
            return RunPythonAsserts(code, asserts);
        }
        catch { return false; }
    }

    private static string ExtractCode(string text)
    {
        var match = Regex.Match(text, @"```(?:python)?\s*(.*?)```", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : text.Trim();
    }

    private static bool RunPythonAsserts(string code, string[] asserts)
    {
        var script = code + "\n\n" + string.Join("\n", asserts) + "\nprint(\"OK\")";
        var tempFile = Path.GetTempFileName() + ".py";
        File.WriteAllText(tempFile, script);

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = tempFile,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return process.ExitCode == 0 && output.Contains("OK");
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}

BenchmarkQuality.Main(args).GetAwaiter().GetResult();
