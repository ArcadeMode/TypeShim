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

    public void Execute(List<string> csFiles, string csOutputDir, string tsOutputFile)
    {
        // Join the file paths with semicolons as the generator expects
        string csFilesArg = string.Join(";", csFiles);

        string arguments = $"\"{csFilesArg}\" \"{csOutputDir}\" \"{tsOutputFile}\"";
        var psi = new ProcessStartInfo
        {
            FileName = _generatorPath,
            Arguments = arguments,
            WorkingDirectory = _projectRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start generator process");
        process.WaitForExit();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        Console.WriteLine(error);
        Console.WriteLine(output);
        if (process.ExitCode != 0)
        {
            //Console.Error.WriteLine($"Generator exited with ExitCode {process.ExitCode}");
            // dont fail, just print
        }
    }
}

public class GeneratorResult
{
    public required bool Success { get; init; }
    public required int ExitCode { get; init; }
    public required string Output { get; init; }
    public required string Error { get; init; }
}
