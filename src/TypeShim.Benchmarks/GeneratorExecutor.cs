using System.Diagnostics;

namespace TypeShim.Benchmarks;

/// <summary>
/// Executes the TypeShim generator and measures performance
/// </summary>
public class GeneratorExecutor
{
    private readonly string _generatorPath;
    private readonly string _projectRoot;

    public GeneratorExecutor(string generatorPath, string projectRoot)
    {
        _generatorPath = generatorPath;
        _projectRoot = projectRoot;
    }

    public async Task<GeneratorResult> ExecuteAsync(List<string> csFiles, string csOutputDir, string tsOutputFile)
    {
        // Create output directories
        Directory.CreateDirectory(csOutputDir);
        Directory.CreateDirectory(Path.GetDirectoryName(tsOutputFile)!);

        // Join the file paths with semicolons as the generator expects
        string csFilesArg = string.Join(";", csFiles);

        // Prepare arguments
        string arguments = $"\"{csFilesArg}\" \"{csOutputDir}\" \"{tsOutputFile}\"";

        // Execute the generator
        var psi = new ProcessStartInfo
        {
            FileName = _generatorPath,
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

        string output = await outputTask;
        string error = await errorTask;

        return new GeneratorResult
        {
            Success = process.ExitCode == 0,
            ExitCode = process.ExitCode,
            Output = output,
            Error = error
        };
    }

    public void Execute(List<string> csFiles, string csOutputDir, string tsOutputFile)
    {
        ExecuteAsync(csFiles, csOutputDir, tsOutputFile).GetAwaiter().GetResult();
    }
}

public class GeneratorResult
{
    public required bool Success { get; init; }
    public required int ExitCode { get; init; }
    public required string Output { get; init; }
    public required string Error { get; init; }
}
