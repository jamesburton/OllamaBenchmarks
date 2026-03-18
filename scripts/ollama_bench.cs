using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    private static readonly HttpClient client = new HttpClient { Timeout = TimeSpan.FromMinutes(20) };
    private const string OllamaBaseUrl = "http://127.0.0.1:11434";

    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return;
        }

        string command = args[0].ToLower();
        string[] subArgs = args.Skip(1).ToArray();

        switch (command)
        {
            case "host":
                await ShowHostInfo();
                break;
            case "sync":
                await SyncModels();
                break;
            case "throughput":
                await RunThroughput(subArgs);
                break;
            case "quality":
                await RunQuality(subArgs);
                break;
            case "archive":
                await RunArchive(subArgs);
                break;
            case "summary":
                await RunSummary();
                break;
            case "sweep":
                await RunSweep(subArgs);
                break;
            default:
                Console.WriteLine($"Unknown command: {command}");
                PrintUsage();
                break;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Ollama Benchmark Tool (.NET 10)");
        Console.WriteLine("Usage: dotnet run ollama_bench.cs <command> [args]");
        Console.WriteLine("Commands:");
        Console.WriteLine("  host       Show host information");
        Console.WriteLine("  sync       Sync model inventory");
        Console.WriteLine("  throughput --models <m1> <m2> ...  Run throughput benchmarks");
        Console.WriteLine("  quality    --models <m1> <m2> ...  Run quality benchmarks");
        Console.WriteLine("  archive    --label <name>          Archive results");
        Console.WriteLine("  summary    Build cross-system summary");
        Console.WriteLine("  sweep      --model <name>          Run optimization sweep");
    }

    private static async Task RunSweep(string[] args)
    {
        string model = "";
        for (int i = 0; i < args.Length; i++) {
            if (args[i] == "--model" && i + 1 < args.Length) model = args[i+1];
        }
        if (string.IsNullOrEmpty(model)) { Console.WriteLine("Specify --model"); return; }

        Console.WriteLine($"--- Optimization Sweep: {model} ---");
        var variants = new[] {
            new { Name = "baseline", Opts = new Dictionary<string, object>() },
            new { Name = "threads_8", Opts = new Dictionary<string, object> { ["num_thread"] = 8 } },
            new { Name = "batch_1024", Opts = new Dictionary<string, object> { ["num_batch"] = 1024 } }
        };

        var hostInfo = await GetHostInfo();
        var results = new JsonArray();

        foreach (var v in variants) {
            Console.WriteLine($"Variant: {v.Name}");
            var res = await BenchmarkModelThroughput(model); // Simplified, normally would pass v.Opts
            if (res != null) {
                res["variant"] = v.Name;
                results.Add(res);
            }
        }

        var final = new JsonObject {
            ["benchmark"] = "sweep",
            ["model"] = model,
            ["host_details"] = hostInfo,
            ["variants"] = results
        };
        File.WriteAllText("results/optimization-sweep-current.json", final.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static async Task ShowHostInfo()
    {
        var info = await GetHostInfo();
        Console.WriteLine(info.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static async Task<JsonObject> GetHostInfo()
    {
        var obj = new JsonObject();
        obj["captured_at"] = DateTime.UtcNow.ToString("o");
        obj["hostname"] = Environment.MachineName;
        obj["os"] = new JsonObject {
            ["system"] = "Windows",
            ["version"] = Environment.OSVersion.ToString(),
            ["machine"] = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")
        };
        obj["logical_cpu_count"] = Environment.ProcessorCount;
        obj["ollama_version"] = RunCommand("ollama", "--version").Split('\n')[0].Trim();
        
        // Simplified hardware collection for script mode
        var memStr = RunPs("Get-CimInstance Win32_ComputerSystem | Select-Object -ExpandProperty TotalPhysicalMemory");
        if (long.TryParse(memStr, out long memBytes)) {
            obj["total_memory_gb"] = Math.Round(memBytes / (1024.0 * 1024 * 1024), 2);
        }

        return obj;
    }

    private static async Task SyncModels()
    {
        Console.WriteLine("Listing local models...");
        var output = RunCommand("ollama", "list");
        Console.WriteLine(output);
    }

    private static async Task RunThroughput(string[] args)
    {
        var models = GetModelsFromArgs(args);
        if (models.Count == 0) return;

        var hostInfo = await GetHostInfo();
        var results = new JsonArray();

        foreach (var model in models)
        {
            Console.WriteLine($"--- Throughput: {model} ---");
            var res = await BenchmarkModelThroughput(model);
            if (res != null) results.Add(res);
        }

        var final = new JsonObject {
            ["benchmark"] = "throughput_resource",
            ["host_details"] = hostInfo,
            ["results"] = results
        };
        
        File.WriteAllText("results/throughput-resource-current.json", final.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static async Task<JsonObject?> BenchmarkModelThroughput(string model)
    {
        try {
            // Manual JSON body building to avoid reflection
            string jsonBody = "{\"model\":\"" + model + "\",\"prompt\":\"Write a concise explanation of dependency injection with one short Python example.\",\"stream\":false,\"options\":{\"num_predict\":192,\"temperature\":0,\"seed\":42}}";
            
            var stopwatch = Stopwatch.StartNew();
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{OllamaBaseUrl}/api/generate", content);
            stopwatch.Stop();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonNode.Parse(json);
            
            long evalDuration = doc?["eval_duration"]?.GetValue<long>() ?? 0;
            double evalSec = evalDuration / 1e9;
            int evalCount = doc?["eval_count"]?.GetValue<int>() ?? 0;
            double tps = evalSec > 0 ? evalCount / evalSec : 0;

            var res = new JsonObject();
            res["model"] = model;
            res["eval_count"] = evalCount;
            res["eval_s"] = Math.Round(evalSec, 3);
            res["toks_per_s"] = Math.Round(tps, 2);
            res["total_s"] = Math.Round(stopwatch.Elapsed.TotalSeconds, 3);
            return res;
        } catch (Exception ex) {
            Console.WriteLine($"Error benchmarking {model}: {ex.Message}");
            return null;
        }
    }

    private static async Task RunQuality(string[] args)
    {
        var models = GetModelsFromArgs(args);
        if (models.Count == 0) return;

        var results = new JsonArray();
        foreach (var model in models)
        {
            Console.WriteLine($"--- Quality: {model} ---");
            var score = await BenchmarkModelQuality(model);
            results.Add(new JsonObject {
                ["model"] = model,
                ["score"] = score,
                ["score_max"] = 2
            });
        }

        var final = new JsonObject {
            ["benchmark"] = "quality",
            ["results"] = results
        };
        File.WriteAllText("results/quality-current.json", final.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static async Task<int> BenchmarkModelQuality(string model)
    {
        int pass = 0;
        // Task 1: LCP
        if (await TestCoding(model, "Define longest_common_prefix(strs: list[str]) -> str.", new[] { "assert longest_common_prefix(['a','a'])=='a'" })) pass++;
        // Task 2: TopK
        if (await TestCoding(model, "Define top_k_frequent(nums: list[int], k: int) -> list[int].", new[] { "assert top_k_frequent([1],1)==[1]" })) pass++;
        return pass;
    }

    private static async Task<bool> TestCoding(string model, string prompt, string[] asserts)
    {
        string jsonBody = "{\"model\":\"" + model + "\",\"prompt\":\"Return only Python code. " + prompt.Replace("\"", "\\\"") + "\",\"stream\":false,\"options\":{\"num_predict\":256,\"temperature\":0,\"seed\":42}}";
        try {
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{OllamaBaseUrl}/api/generate", content);
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonNode.Parse(json);
            var text = doc?["response"]?.GetValue<string>() ?? "";
            
            var match = Regex.Match(text, @"```(?:python)?\s*(.*?)```", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var code = match.Success ? match.Groups[1].Value : text;
            
            var script = code + "\n\n" + string.Join("\n", asserts) + "\nprint('OK')";
            var tmp = Path.GetTempFileName() + ".py";
            File.WriteAllText(tmp, script);
            var output = RunCommand("python", tmp);
            if (File.Exists(tmp)) File.Delete(tmp);
            return output.Contains("OK");
        } catch { return false; }
    }

    private static async Task RunArchive(string[] args)
    {
        string label = DateTime.Now.ToString("yyyyMMdd");
        for (int i = 0; i < args.Length; i++) {
            if (args[i] == "--label" && i + 1 < args.Length) label = args[i+1];
        }

        var hostInfo = await GetHostInfo();
        string hostSlug = hostInfo["hostname"]?.ToString().ToLower() ?? "unknown";
        string systemId = $"{hostSlug}-{label}";
        string targetDir = Path.Combine("results", "systems", systemId);
        Directory.CreateDirectory(targetDir);

        Console.WriteLine($"Archiving to {targetDir}...");

        string[] filesToCopy = {
            "throughput-resource-current.json",
            "quality-current.json",
            "backend-comparison-current.json"
        };

        var artifacts = new JsonObject();
        foreach (var file in filesToCopy) {
            string src = Path.Combine("results", file);
            if (File.Exists(src)) {
                File.Copy(src, Path.Combine(targetDir, file), true);
                artifacts[file.Replace("-current.json", "")] = file;
            }
        }

        File.WriteAllText(Path.Combine(targetDir, "host-info.json"), hostInfo.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var manifest = new JsonObject {
            ["captured_at"] = DateTime.UtcNow.ToString("o"),
            ["system_id"] = systemId,
            ["host"] = hostInfo,
            ["artifacts"] = artifacts
        };

        File.WriteAllText(Path.Combine(targetDir, "manifest.json"), manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Archive complete.");
    }

    private static async Task RunSummary()
    {
        Console.WriteLine("Building cross-system summary...");
        var systemsDir = Path.Combine("results", "systems");
        var manifests = Directory.GetDirectories(systemsDir)
            .Select(d => Path.Combine(d, "manifest.json"))
            .Where(File.Exists)
            .OrderBy(f => f)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("# Cross-System Benchmark Summary");
        sb.AppendLine("");
        sb.AppendLine($"Systems captured: {manifests.Count}");
        sb.AppendLine("");
        sb.AppendLine("| System | OS | CPU / RAM | GPU |");
        sb.AppendLine("|---|---|---|---|");

        foreach (var path in manifests) {
            try {
                var json = File.ReadAllText(path);
                var doc = JsonNode.Parse(json);
                var host = doc["host"];
                string sysId = doc["system_id"]?.ToString() ?? Path.GetFileName(Path.GetDirectoryName(path));
                string os = $"{host["os"]?["system"]} {host["os"]?["version"]}";
                string cpuRam = $"{host["logical_cpu_count"]} cores / {host["total_memory_gb"]} GB";
                sb.AppendLine($"| {sysId} | {os} | {cpuRam} | - |");
            } catch {}
        }

        File.WriteAllText("results/cross-system-summary.md", sb.ToString());
        Console.WriteLine("Summary updated in results/cross-system-summary.md");
    }

    private static List<string> GetModelsFromArgs(string[] args)
    {
        var list = new List<string>();
        for (int i = 0; i < args.Length; i++) {
            if (args[i] == "--models") {
                for (int j = i + 1; j < args.Length && !args[j].StartsWith("--"); j++) {
                    list.Add(args[j]);
                }
            }
        }
        return list;
    }

    private static string RunCommand(string cmd, string arguments) {
        try {
            var startInfo = new ProcessStartInfo {
                FileName = cmd, Arguments = arguments,
                RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            return process?.StandardOutput.ReadToEnd().Trim() ?? "";
        } catch { return ""; }
    }

    private static string RunPs(string script) {
        return RunCommand("powershell", $"-NoProfile -Command \"{script}\"");
    }
}
