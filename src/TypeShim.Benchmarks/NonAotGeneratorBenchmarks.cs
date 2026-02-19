using BenchmarkDotNet.Attributes;

namespace TypeShim.Benchmarks;

/// <summary>
/// Benchmarks for Non-AOT generator builds
/// </summary>
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class NonAotGeneratorBenchmarks
{
    private GeneratorSetup _setup = null!;
    private GeneratorExecutor _executor = null!;
    private string _tempDir = null!;
    private Dictionary<int, List<string>> _pregeneratedClassFiles = null!;

    [GlobalSetup]
    public void Setup()
    {
        _setup = new GeneratorSetup();
        _setup.Validate();
        _executor = new GeneratorExecutor(_setup.NonAotGeneratorPath, _setup.ProjectRoot);
        _tempDir = Path.Combine(Path.GetTempPath(), "TypeShimBenchmark_NonAOT");
        
        // Clean up any previous benchmark runs
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
        Directory.CreateDirectory(_tempDir);

        // Pre-generate class files for each class count to avoid noise in benchmark results
        _pregeneratedClassFiles = new Dictionary<int, List<string>>();
        int[] classCounts = [1, 10, 25, 50, 100];
        
        foreach (int classCount in classCounts)
        {
            string tempClassesDir = Path.Combine(_tempDir, $"pregenerated_{classCount}");
            var csFiles = _setup.GenerateClassFiles(classCount, tempClassesDir);
            _pregeneratedClassFiles[classCount] = csFiles;
        }
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
    [Arguments(1)]
    public void GenerateCode_01Class(int classCount)
    {
        RunBenchmark(classCount);
    }

    [Benchmark]
    [Arguments(10)]
    public void GenerateCode_10Classes(int classCount)
    {
        RunBenchmark(classCount);
    }

    [Benchmark]
    [Arguments(25)]
    public void GenerateCode_25Classes(int classCount)
    {
        RunBenchmark(classCount);
    }

    [Benchmark]
    [Arguments(50)]
    public void GenerateCode_50Classes(int classCount)
    {
        RunBenchmark(classCount);
    }

    [Benchmark]
    [Arguments(100)]
    public void GenerateCode_100Classes(int classCount)
    {
        RunBenchmark(classCount);
    }

    private void RunBenchmark(int classCount)
    {
        string runDir = Path.Combine(_tempDir, $"run_{classCount}_{Guid.NewGuid():N}");
        string csOutputDir = Path.Combine(runDir, "cs");
        string tsOutputFile = Path.Combine(runDir, "ts", "output.ts");

        var csFiles = _pregeneratedClassFiles[classCount];
        _executor.Execute(csFiles, csOutputDir, tsOutputFile);
    }
}
