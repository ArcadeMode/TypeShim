using BenchmarkDotNet.Attributes;

namespace TypeShim.Benchmarks;

//[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class GeneratorBenchmarks
{
    private GeneratorSetup _setup = null!;
    private GeneratorExecutor _executor = null!;
    private string _tempDir = null!;
    private Dictionary<int, List<string>> _pregeneratedClassFiles = null!;

    public enum Mode { Native, Managed }

    [Params(Mode.Native, Mode.Managed)]
    public Mode TestMode { get; set; }

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
        int[] classCounts = [1, 10, 25, 50, 100];
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
        _executor = new GeneratorExecutor(TestMode == Mode.Native ? _setup.AotGeneratorPath : _setup.NonAotGeneratorPath, _setup.ProjectRoot);
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
    public async Task GenerateCode_01Class(int classCount)
    {
        RunBenchmark(classCount);
    }

    [Benchmark]
    [Arguments(10)]
    public async Task GenerateCode_10Classes(int classCount)
    {
        RunBenchmark(classCount);
    }

    [Benchmark]
    [Arguments(25)]
    public async Task GenerateCode_25Classes(int classCount)
    {
        RunBenchmark(classCount);
    }

    [Benchmark]
    [Arguments(50)]
    public async Task GenerateCode_50Classes(int classCount)
    {
        RunBenchmark(classCount);
    }

    [Benchmark]
    [Arguments(100)]
    public async Task GenerateCode_100Classes(int classCount)
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
