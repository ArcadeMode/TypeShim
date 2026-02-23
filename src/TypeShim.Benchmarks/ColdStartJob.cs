using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
namespace TypeShim.Benchmarks;

public sealed class ColdStartConfig : ManualConfig
{
    public ColdStartConfig()
    {
        Add(DefaultConfig.Instance);
        AddJob(Job.Default
            .WithStrategy(RunStrategy.ColdStart)
            .WithWarmupCount(0) // no warm up - users dont get this either
            .WithIterationCount(1)
            .WithLaunchCount(10));
    }
}