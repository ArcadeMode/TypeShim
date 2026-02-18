# TypeShim.Benchmarks

This project benchmarks the performance of TypeShim.Generator in both AOT (Ahead-of-Time) and non-AOT compilation modes.

## Overview

The benchmark measures code generation duration for varying numbers of classes (1, 10, 25, 50, and 100) to compare:
- Non-AOT build performance
- AOT build performance
- Relative speedup achieved with AOT compilation

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

## Output Format

The benchmark outputs:
1. Progress for each test run
2. A summary table comparing performance:
   - Duration for Non-AOT builds
   - Duration for AOT builds
   - Improvement (time saved)
   - Speedup multiplier

Example output:
```
=== BENCHMARK SUMMARY ===

| Classes | Non-AOT (ms) | AOT (ms) | Improvement | Speedup |
|---------|--------------|----------|-------------|---------|
|       1 |       150.25 |    75.50 |       74.75 |    1.99x |
|      10 |       320.50 |   165.25 |      155.25 |    1.94x |
|      25 |       650.75 |   340.10 |      310.65 |    1.91x |
|      50 |      1200.30 |   625.45 |      574.85 |    1.92x |
|     100 |      2350.60 |  1220.80 |     1129.80 |    1.93x |

Average Non-AOT: 934.28 ms
Average AOT: 485.42 ms
Average Speedup: 1.92x
```

## Dependencies

The benchmark project only depends on:
- **TypeShim** - For the `TSExportAttribute` used to mark sample classes

This minimal dependency structure ensures the benchmark focuses purely on generator performance.

## Project Structure

```
TypeShim.Benchmarks/
├── TypeShim.Benchmarks.csproj  # Project file
├── Program.cs                   # Entry point
├── BenchmarkRunner.cs           # Main benchmark logic
├── SampleClasses/               # Sample classes for testing
│   └── SampleClass.cs          # 10 sample classes
├── Scripts/                     # Build scripts
│   ├── build-generators.sh     # Linux/Mac build script
│   └── build-generators.ps1    # Windows build script
└── README.md                    # This file
```
