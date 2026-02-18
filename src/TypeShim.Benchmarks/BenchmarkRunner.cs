using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace TypeShim.Benchmarks;

/// <summary>
/// Benchmark runner for testing TypeShim.Generator performance
/// </summary>
public class BenchmarkRunner
{
    private readonly string _projectRoot;
    private readonly string _benchmarkProjectSourceDir;
    private readonly string _generatorBuildsDir;
    private readonly string _tempOutputDir;

    public BenchmarkRunner()
    {
        // Get the project root directory
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        string binDir = Path.GetDirectoryName(assemblyLocation)!;
        
        // Navigate from bin/Release/net10.0 up to the project directory
        _benchmarkProjectSourceDir = Path.GetFullPath(Path.Combine(binDir, "../../.."));
        _projectRoot = Path.GetFullPath(Path.Combine(_benchmarkProjectSourceDir, "../.."));
        _generatorBuildsDir = Path.Combine(_benchmarkProjectSourceDir, "GeneratorBuilds");
        _tempOutputDir = Path.Combine(Path.GetTempPath(), "TypeShimBenchmark");
    }

    public async Task RunBenchmarksAsync()
    {
        Console.WriteLine("=== TypeShim Generator Benchmark ===");
        Console.WriteLine($"Project Root: {_projectRoot}");
        Console.WriteLine();

        // Verify generator builds exist
        if (!Directory.Exists(_generatorBuildsDir))
        {
            Console.WriteLine("Error: Generator builds not found. Please run the build script first:");
            Console.WriteLine("  Linux/Mac: ./Scripts/build-generators.sh");
            Console.WriteLine("  Windows:   .\\Scripts\\build-generators.ps1");
            return;
        }

        string nonAotGeneratorPath = GetGeneratorPath("NonAOT");
        string aotGeneratorPath = GetGeneratorPath("AOT");

        if (!File.Exists(nonAotGeneratorPath))
        {
            Console.WriteLine($"Error: Non-AOT generator not found at {nonAotGeneratorPath}");
            return;
        }

        if (!File.Exists(aotGeneratorPath))
        {
            Console.WriteLine($"Error: AOT generator not found at {aotGeneratorPath}");
            return;
        }

        Console.WriteLine($"Non-AOT Generator: {nonAotGeneratorPath}");
        Console.WriteLine($"AOT Generator: {aotGeneratorPath}");
        Console.WriteLine();

        // Build the benchmark project to get the assembly with sample classes
        Console.WriteLine("Building benchmark project...");
        if (!await BuildBenchmarkProjectAsync())
        {
            Console.WriteLine("Error: Failed to build benchmark project");
            return;
        }

        string benchmarkAssembly = Path.Combine(_benchmarkProjectSourceDir, "bin/Release/net10.0/TypeShim.Benchmarks.dll");
        if (!File.Exists(benchmarkAssembly))
        {
            Console.WriteLine($"Error: Benchmark assembly not found at {benchmarkAssembly}");
            return;
        }

        Console.WriteLine($"Benchmark Assembly: {benchmarkAssembly}");
        Console.WriteLine();

        // Run benchmarks
        int[] classCounts = [1, 10, 25, 50, 100];
        var results = new List<BenchmarkResult>();

        foreach (int classCount in classCounts)
        {
            Console.WriteLine($"=== Benchmarking with {classCount} class(es) ===");
            Console.WriteLine();

            // Non-AOT benchmark
            var nonAotResult = await RunGeneratorBenchmarkAsync(
                nonAotGeneratorPath, 
                benchmarkAssembly, 
                classCount, 
                "Non-AOT");
            results.Add(nonAotResult);

            // AOT benchmark
            var aotResult = await RunGeneratorBenchmarkAsync(
                aotGeneratorPath, 
                benchmarkAssembly, 
                classCount, 
                "AOT");
            results.Add(aotResult);

            Console.WriteLine();
        }

        // Display summary
        DisplaySummary(results);
    }

    private string GetGeneratorPath(string buildType)
    {
        string generatorDir = Path.Combine(_generatorBuildsDir, buildType);
        
        // On Windows, look for .exe, on Linux/Mac look for the executable without extension
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(generatorDir, "TypeShim.Generator.exe");
        }
        else
        {
            return Path.Combine(generatorDir, "TypeShim.Generator");
        }
    }

    private async Task<bool> BuildBenchmarkProjectAsync()
    {
        string projectFile = Path.Combine(_projectRoot, "src/TypeShim.Benchmarks/TypeShim.Benchmarks.csproj");
        
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{projectFile}\" -c Release",
            WorkingDirectory = _projectRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            Console.WriteLine("Error: Failed to start build process");
            return false;
        }

        await process.WaitForExitAsync();
        return process.ExitCode == 0;
    }

    private async Task<BenchmarkResult> RunGeneratorBenchmarkAsync(
        string generatorPath, 
        string assemblyPath, 
        int classCount, 
        string buildType)
    {
        // Create temp output directories
        string outputDir = Path.Combine(_tempOutputDir, $"{buildType}_{classCount}");
        string csOutputDir = Path.Combine(outputDir, "cs");
        string tsOutputFile = Path.Combine(outputDir, "ts", "output.ts");
        string tempClassesDir = Path.Combine(outputDir, "temp_classes");
        
        Directory.CreateDirectory(csOutputDir);
        Directory.CreateDirectory(Path.GetDirectoryName(tsOutputFile)!);
        Directory.CreateDirectory(tempClassesDir);

        // Generate the specified number of classes
        // We'll use the base sample classes and duplicate/modify them to reach the target count
        string sampleClassesDir = Path.Combine(_benchmarkProjectSourceDir, "SampleClasses");
        string[] baseCsFiles = Directory.GetFiles(sampleClassesDir, "*.cs");
        
        if (baseCsFiles.Length == 0)
        {
            throw new InvalidOperationException($"No .cs files found in {sampleClassesDir}");
        }

        // Read the base file which contains 10 sample classes
        string baseContent = File.ReadAllText(baseCsFiles[0]);
        
        // Create files with the required number of classes
        // We multiply the classes by changing their names
        var tempFiles = new List<string>();
        int classesPerFile = 10;
        int filesNeeded = (classCount + classesPerFile - 1) / classesPerFile; // Ceiling division
        
        for (int fileIndex = 0; fileIndex < filesNeeded; fileIndex++)
        {
            string content = baseContent;
            
            // Replace class names to create unique classes
            if (fileIndex > 0)
            {
                for (int classNum = 1; classNum <= classesPerFile; classNum++)
                {
                    string oldClassName = $"SampleClass{classNum:D2}";
                    string newClassName = $"SampleClass{fileIndex}_{classNum:D2}";
                    content = content.Replace($"class {oldClassName}", $"class {newClassName}");
                    content = content.Replace($"static {oldClassName} Create()", $"static {newClassName} Create()");
                    content = content.Replace($"return new {oldClassName}", $"return new {newClassName}");
                    content = content.Replace($"private {oldClassName}(", $"private {newClassName}(");
                }
            }
            
            string tempFilePath = Path.Combine(tempClassesDir, $"SampleClasses_{fileIndex}.cs");
            File.WriteAllText(tempFilePath, content);
            tempFiles.Add(tempFilePath);
        }

        // Join the file paths with semicolons as the generator expects
        string csFilesArg = string.Join(";", tempFiles);

        // Prepare arguments
        string arguments = $"\"{csFilesArg}\" \"{csOutputDir}\" \"{tsOutputFile}\"";

        // Measure execution time
        var stopwatch = Stopwatch.StartNew();
        
        var psi = new ProcessStartInfo
        {
            FileName = generatorPath,
            Arguments = arguments,
            WorkingDirectory = _projectRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start generator process");
        }

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();
        stopwatch.Stop();

        string output = await outputTask;
        string error = await errorTask;

        bool success = process.ExitCode == 0;

        if (!success)
        {
            Console.WriteLine($"Generator failed with exit code {process.ExitCode}");
            Console.WriteLine($"Error: {error}");
        }

        var result = new BenchmarkResult
        {
            BuildType = buildType,
            ClassCount = classCount,
            Duration = stopwatch.Elapsed,
            Success = success
        };

        Console.WriteLine($"{buildType} - {classCount} class(es): {result.Duration.TotalMilliseconds:F2} ms");

        return result;
    }

    private void DisplaySummary(List<BenchmarkResult> results)
    {
        Console.WriteLine("=== BENCHMARK SUMMARY ===");
        Console.WriteLine();

        var grouped = results
            .Where(r => r.Success)
            .GroupBy(r => r.ClassCount)
            .OrderBy(g => g.Key);

        Console.WriteLine("| Classes | Non-AOT (ms) | AOT (ms) | Improvement | Speedup |");
        Console.WriteLine("|---------|--------------|----------|-------------|---------|");

        foreach (var group in grouped)
        {
            var nonAot = group.FirstOrDefault(r => r.BuildType == "Non-AOT");
            var aot = group.FirstOrDefault(r => r.BuildType == "AOT");

            if (nonAot != null && aot != null)
            {
                double improvement = nonAot.Duration.TotalMilliseconds - aot.Duration.TotalMilliseconds;
                double speedup = nonAot.Duration.TotalMilliseconds / aot.Duration.TotalMilliseconds;

                Console.WriteLine($"| {group.Key,7} | {nonAot.Duration.TotalMilliseconds,12:F2} | {aot.Duration.TotalMilliseconds,8:F2} | {improvement,11:F2} | {speedup,7:F2}x |");
            }
        }

        Console.WriteLine();

        // Calculate averages
        var allNonAot = results.Where(r => r.Success && r.BuildType == "Non-AOT").ToList();
        var allAot = results.Where(r => r.Success && r.BuildType == "AOT").ToList();

        if (allNonAot.Any() && allAot.Any())
        {
            double avgNonAot = allNonAot.Average(r => r.Duration.TotalMilliseconds);
            double avgAot = allAot.Average(r => r.Duration.TotalMilliseconds);
            double avgSpeedup = avgNonAot / avgAot;

            Console.WriteLine($"Average Non-AOT: {avgNonAot:F2} ms");
            Console.WriteLine($"Average AOT: {avgAot:F2} ms");
            Console.WriteLine($"Average Speedup: {avgSpeedup:F2}x");
        }
    }
}

public class BenchmarkResult
{
    public required string BuildType { get; init; }
    public required int ClassCount { get; init; }
    public required TimeSpan Duration { get; init; }
    public required bool Success { get; init; }
}
