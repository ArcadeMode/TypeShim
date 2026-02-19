using BenchmarkDotNet.Running;
using TypeShim.Benchmarks;

try
{
    GeneratorSetup setup = new();
    setup.Validate();
    // Set environment variable so BenchmarkDotNet subprocesses can find generator builds
    Environment.SetEnvironmentVariable("TYPESHIM_GENERATOR_BUILDS_DIR", setup.GeneratorBuildsDir);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Setup validation failed: {ex.Message}");
    return 1;
}

Console.WriteLine("=== TypeShim Generator Benchmarks Starting ===");
Console.WriteLine();

BenchmarkRunner.Run<GeneratorBenchmarks>(new ColdStartConfig());

Console.WriteLine("=== TypeShim Generator Benchmarks Completed ===");
return 0;
