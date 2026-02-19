using BenchmarkDotNet.Running;
using System.Reflection;
using TypeShim.Benchmarks;

// Validate setup before running benchmarks
var setup = new GeneratorSetup();
try
{
    setup.Validate();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Setup validation failed: {ex.Message}");
    return 1;
}

Console.WriteLine("=== TypeShim Generator Benchmarks ===");
Console.WriteLine($"Non-AOT Generator: {setup.NonAotGeneratorPath}");
Console.WriteLine($"AOT Generator: {setup.AotGeneratorPath}");
Console.WriteLine();

// Set environment variable so BenchmarkDotNet subprocesses can find generator builds
Environment.SetEnvironmentVariable("TYPESHIM_GENERATOR_BUILDS_DIR", setup.GeneratorBuildsDir);

// Run benchmarks
BenchmarkRunner.Run<NonAotGeneratorBenchmarks>();
BenchmarkRunner.Run<AotGeneratorBenchmarks>();

return 0;
