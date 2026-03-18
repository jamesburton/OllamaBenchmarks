using System;
using System.Diagnostics;
using System.Text.Json;
using System.Collections.Generic;

var compact = args.Length > 0 && args[0] == "--compact";

string RunCommand(string cmd, string arguments) {
    try {
        var startInfo = new ProcessStartInfo {
            FileName = cmd,
            Arguments = arguments,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = Process.Start(startInfo);
        using var reader = process.StandardOutput;
        return reader.ReadToEnd().Trim();
    } catch { return ""; }
}

string RunPs(string script) {
    return RunCommand("powershell", $"-NoProfile -Command \"{script}\"");
}

var hostname = Environment.MachineName;
var os = Environment.OSVersion;
var cpuCount = Environment.ProcessorCount;
var memInfo = RunPs("Get-CimInstance Win32_ComputerSystem | Select-Object Manufacturer,Model,TotalPhysicalMemory | ConvertTo-Json -Compress");
var gpus = RunPs("Get-CimInstance Win32_VideoController | Select-Object Name,AdapterRAM,DriverVersion,VideoProcessor,Status | ConvertTo-Json -Compress");
var ollamaVer = RunCommand("ollama", "--version").Split('\n')[0];

var payload = new Dictionary<string, object> {
    ["captured_at"] = DateTime.UtcNow.ToString("o"),
    ["hostname"] = hostname,
    ["os"] = new {
        system = "Windows",
        release = os.VersionString,
        version = os.Version.ToString(),
        machine = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")
    },
    ["logical_cpu_count"] = cpuCount,
    ["ollama_version"] = ollamaVer,
    ["hardware_raw"] = memInfo,
    ["gpus_raw"] = gpus
};

// Simplified JSON output for now to avoid Reflection-based issues
if (compact) {
    Console.WriteLine("{\"hostname\":\"" + hostname + "\"}");
} else {
    Console.WriteLine("{");
    Console.WriteLine("  \"hostname\": \"" + hostname + "\",");
    Console.WriteLine("  \"captured_at\": \"" + DateTime.UtcNow.ToString("o") + "\",");
    Console.WriteLine("  \"ollama_version\": \"" + ollamaVer.Replace("\"", "\\\"") + "\"");
    Console.WriteLine("}");
}
