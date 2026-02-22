using BenchmarkDotNet.Attributes;

namespace TypeShim.Benchmarks;

[MinColumn, MaxColumn]
public class GeneratorBenchmarks
{
    private GeneratorSetup _setup = null!;
    private GeneratorExecutor _executor = null!;
    private string _tempDir = null!;
    private Dictionary<int, List<string>> _pregeneratedClassFiles = null!;

    public enum CompilationMode { AOT, JIT }

    [Params(CompilationMode.AOT, CompilationMode.JIT)]
    public CompilationMode Compilation { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _setup = new GeneratorSetup();
        _setup.Validate();
        _tempDir = Path.Combine(AppContext.BaseDirectory, "BenchmarkExecution");
        
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
        Directory.CreateDirectory(_tempDir);

        _pregeneratedClassFiles = [];
        int[] classCounts = [1, 10, 25, 50, 100, 200];
        foreach (int classCount in classCounts)
        {
            string tempClassesDir = Path.Combine(_tempDir, $"pregenerated_{classCount}");
            var csFiles = _setup.GenerateClassFiles(classCount, tempClassesDir);
            _pregeneratedClassFiles[classCount] = csFiles;
        }
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _executor = new GeneratorExecutor(Compilation == CompilationMode.AOT ? _setup.AotGeneratorPath : _setup.NonAotGeneratorPath, _setup.ProjectRoot);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
        {
            try
            {
                Directory.Delete(_tempDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Benchmark]
    [Arguments(0)] // determine process-start overhead
    [Arguments(1)]
    [Arguments(10)]
    [Arguments(25)]
    [Arguments(50)]
    [Arguments(100)]
    [Arguments(200)]
    public void Generate(int ClassCount)
    {
        if (ClassCount == 0)
        {
            _executor.Execute([], "/", "/");
        }
        else
        {
            RunBenchmark(ClassCount);
        }
    }

    private void RunBenchmark(int classCount)
    {
        string runDir = Path.Combine(_tempDir, $"run_{classCount}_{Guid.NewGuid():N}");
        string csOutputDir = Path.Combine(runDir, "cs");
        string tsOutputFile = Path.Combine(runDir, "ts", "output.ts");
        Directory.CreateDirectory(csOutputDir);
        Directory.CreateDirectory(Path.GetDirectoryName(tsOutputFile)!);
        var csFiles = _pregeneratedClassFiles[classCount];
        _executor.Execute(csFiles, csOutputDir, tsOutputFile);
    }
}
