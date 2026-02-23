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
        string packRefDir = Environment.GetEnvironmentVariable("TYPESHIM_TARGETINGPACK_REF_DIR") ?? throw new InvalidOperationException("TYPESHIM_TARGETINGPACK_REF_DIR was not set");
        string csFilesArg = string.Join(";", csFiles);

        string arguments = $"\"{csFilesArg}\" \"{csOutputDir}\" \"{tsOutputFile}\" \"{packRefDir}\"";
        var psi = new ProcessStartInfo
        {
            FileName = _generatorPath,
            Arguments = arguments,
            WorkingDirectory = _projectRoot,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            UseShellExecute = false,
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start generator process");
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.Error.WriteLine($"Generator exited with ExitCode {process.ExitCode}");
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
