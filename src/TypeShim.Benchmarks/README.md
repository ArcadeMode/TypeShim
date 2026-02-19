# TypeShim.Benchmarks

This project benchmarks the performance of TypeShim.Generator in both AOT (Ahead-of-Time) and non-AOT compilation modes using **BenchmarkDotNet**.

## Overview

The benchmark measures code generation duration for varying numbers of classes (1, 10, 25, 50, and 100) to compare:
- Non-AOT build performance
- AOT build performance  
- Relative speedup achieved with AOT compilation

BenchmarkDotNet provides high-quality, statistically sound performance measurements with:
- Multiple warmup and measurement iterations
- Memory allocation diagnostics
- Statistical analysis (mean, median, min, max, standard deviation)
- Outlier detection and removal

## Sample Classes

The benchmark includes 10 sample classes with various signatures demonstrating:
- Basic methods and properties
- Static methods
- Various data types (int, long, float, double, bool, string)
- Nullable types
- Constructors and factory methods
- Collections and arrays
- Multiple parameters and return types
- Comprehensive XML documentation comments

## Architecture

The benchmark is organized into separate, coherent classes:

- **`GeneratorSetup`** - Manages paths and validates generator builds
- **`GeneratorExecutor`** - Executes the generator and captures results
- **`NonAotGeneratorBenchmarks`** - BenchmarkDotNet benchmarks for non-AOT builds
- **`AotGeneratorBenchmarks`** - BenchmarkDotNet benchmarks for AOT builds
- **`Program.cs`** - Entry point that validates setup and runs benchmarks

## Running the Benchmark

### Step 1: Build the Generator Projects

First, build both AOT and non-AOT versions of the generator:

**On Linux/Mac:**
```bash
cd src/TypeShim.Benchmarks/Scripts
./build-generators.sh
```

**On Windows (PowerShell):**
```powershell
cd src\TypeShim.Benchmarks\Scripts
.\build-generators.ps1
```

This will create two builds in `src/TypeShim.Benchmarks/GeneratorBuilds/`:
- `NonAOT/` - Standard .NET build
- `AOT/` - Native AOT compiled build

### Step 2: Run the Benchmark

```bash
cd src/TypeShim.Benchmarks
dotnet run -c Release
```

**Note**: BenchmarkDotNet benchmarks must run in Release mode for accurate results.

## Output Format

BenchmarkDotNet produces comprehensive output including:

1. **Per-benchmark results**: Mean, median, min, max execution times
2. **Memory diagnostics**: Allocations and GC collections
3. **Statistical analysis**: Standard deviation, confidence intervals
4. **Comparison tables**: Side-by-side performance comparison
5. **Artifacts**: Detailed reports in `BenchmarkDotNet.Artifacts/`

Example output:
```
| Method                  | classCount | Mean      | Error    | StdDev   | Min       | Max       | Median    | Allocated |
|------------------------ |----------- |----------:|---------:|---------:|----------:|----------:|----------:|----------:|
| GenerateCode_01Class    | 1          |  19.52 ms | 0.145 ms | 0.136 ms |  19.34 ms |  19.78 ms |  19.49 ms |   1.23 MB |
| GenerateCode_10Classes  | 10         |  20.15 ms | 0.187 ms | 0.175 ms |  19.89 ms |  20.45 ms |  20.11 ms |   1.45 MB |
| GenerateCode_25Classes  | 25         |  22.34 ms | 0.234 ms | 0.219 ms |  21.98 ms |  22.67 ms |  22.28 ms |   1.89 MB |
| GenerateCode_50Classes  | 50         |  24.12 ms | 0.198 ms | 0.185 ms |  23.87 ms |  24.45 ms |  24.09 ms |   2.56 MB |
| GenerateCode_100Classes | 100        |  28.45 ms | 0.267 ms | 0.250 ms |  28.11 ms |  28.89 ms |  28.39 ms |   3.78 MB |
```

## Dependencies

The benchmark project depends on:
- **TypeShim** - For the `TSExportAttribute` used to mark sample classes
- **BenchmarkDotNet** - For high-quality performance benchmarking

## Project Structure

```
TypeShim.Benchmarks/
├── TypeShim.Benchmarks.csproj      # Project file
├── Program.cs                       # Entry point and benchmark runner
├── GeneratorSetup.cs                # Setup and path management
├── GeneratorExecutor.cs             # Generator execution logic
├── NonAotGeneratorBenchmarks.cs    # Non-AOT benchmarks
├── AotGeneratorBenchmarks.cs       # AOT benchmarks
├── SampleClasses/                   # Sample classes for testing
│   └── SampleClass.cs              # 10 sample classes
├── Scripts/                         # Build scripts
│   ├── build-generators.sh         # Linux/Mac build script
│   └── build-generators.ps1        # Windows build script
└── README.md                        # This file
```
