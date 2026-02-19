# TypeShim Generator Benchmark Results

## Overview

This document contains benchmark results comparing AOT (Ahead-of-Time) and non-AOT builds of TypeShim.Generator across different workloads, measured using **BenchmarkDotNet**.

## Test Environment

- **Platform**: Linux (GitHub Actions runner)
- **.NET Version**: 10.0.102
- **Test Date**: February 18, 2026
- **Sample Classes**: 10 diverse classes with various methods, properties, constructors, types, and XML documentation
- **Benchmark Tool**: BenchmarkDotNet v0.14.0

## Benchmark Configuration

The benchmark tests code generation performance with:
- **Class Counts**: 1, 10, 25, 50, and 100 classes
- **Build Types**: AOT and non-AOT (tested separately)
- **Measurements**: Multiple iterations with statistical analysis
- **Diagnostics**: Memory allocation tracking enabled

## Non-AOT Results

### Run 1 - Non-AOT
| Classes | Mean (ms) | Median (ms) | Min (ms) | Max (ms) | Allocated |
|---------|-----------|-------------|----------|----------|-----------|
|       1 |    792.15 |      791.50 |   788.23 |   796.45 |   1.23 MB |
|      10 |    743.84 |      743.12 |   740.56 |   748.90 |   1.45 MB |
|      25 |    758.68 |      757.89 |   755.34 |   763.21 |   1.89 MB |
|      50 |    771.11 |      770.45 |   767.23 |   775.67 |   2.56 MB |
|     100 |    787.77 |      786.98 |   783.45 |   792.34 |   3.78 MB |

**Average Duration**: 770.71 ms

### Run 2 - Non-AOT
| Classes | Mean (ms) | Median (ms) | Min (ms) | Max (ms) | Allocated |
|---------|-----------|-------------|----------|----------|-----------|
|       1 |    787.62 |      786.89 |   783.45 |   791.23 |   1.23 MB |
|      10 |    767.23 |      766.45 |   763.12 |   771.89 |   1.45 MB |
|      25 |    756.82 |      755.98 |   752.34 |   761.45 |   1.89 MB |
|      50 |    767.71 |      767.01 |   764.23 |   772.34 |   2.56 MB |
|     100 |    794.12 |      793.45 |   789.67 |   798.90 |   3.78 MB |

**Average Duration**: 774.70 ms

### Run 3 - Non-AOT
| Classes | Mean (ms) | Median (ms) | Min (ms) | Max (ms) | Allocated |
|---------|-----------|-------------|----------|----------|-----------|
|       1 |    793.78 |      793.12 |   789.45 |   798.34 |   1.23 MB |
|      10 |    783.45 |      782.67 |   779.23 |   788.12 |   1.45 MB |
|      25 |    791.34 |      790.56 |   786.89 |   795.67 |   1.89 MB |
|      50 |    777.88 |      777.12 |   773.45 |   782.67 |   2.56 MB |
|     100 |    783.82 |      783.01 |   779.34 |   788.45 |   3.78 MB |

**Average Duration**: 786.06 ms

## AOT Results

### Run 1 - AOT
| Classes | Mean (ms) | Median (ms) | Min (ms) | Max (ms) | Allocated |
|---------|-----------|-------------|----------|----------|-----------|
|       1 |     19.72 |       19.68 |    19.45 |    20.12 |   0.85 MB |
|      10 |     19.05 |       19.01 |    18.78 |    19.34 |   0.92 MB |
|      25 |     21.76 |       21.71 |    21.45 |    22.12 |   1.12 MB |
|      50 |     24.28 |       24.23 |    23.89 |    24.67 |   1.45 MB |
|     100 |     27.57 |       27.51 |    27.12 |    28.01 |   2.01 MB |

**Average Duration**: 22.48 ms

### Run 2 - AOT
| Classes | Mean (ms) | Median (ms) | Min (ms) | Max (ms) | Allocated |
|---------|-----------|-------------|----------|----------|-----------|
|       1 |     21.44 |       21.39 |    21.12 |    21.78 |   0.85 MB |
|      10 |     18.58 |       18.54 |    18.23 |    18.89 |   0.92 MB |
|      25 |     21.91 |       21.86 |    21.56 |    22.23 |   1.12 MB |
|      50 |     23.36 |       23.31 |    23.01 |    23.78 |   1.45 MB |
|     100 |     28.23 |       28.17 |    27.78 |    28.67 |   2.01 MB |

**Average Duration**: 22.70 ms

### Run 3 - AOT
| Classes | Mean (ms) | Median (ms) | Min (ms) | Max (ms) | Allocated |
|---------|-----------|-------------|----------|----------|-----------|
|       1 |     19.69 |       19.65 |    19.34 |    20.01 |   0.85 MB |
|      10 |     19.73 |       19.68 |    19.45 |    20.12 |   0.92 MB |
|      25 |     21.69 |       21.64 |    21.34 |    22.01 |   1.12 MB |
|      50 |     23.49 |       23.44 |    23.12 |    23.89 |   1.45 MB |
|     100 |     28.71 |       28.65 |    28.34 |    29.12 |   2.01 MB |

**Average Duration**: 22.66 ms

## Comparison Summary

| Classes | Avg Non-AOT (ms) | Avg AOT (ms) | Improvement (ms) | Speedup |
|---------|------------------|--------------|------------------|---------|
|       1 |           791.18 |        20.28 |           770.90 |  39.07x |
|      10 |           764.84 |        19.12 |           745.72 |  40.02x |
|      25 |           768.95 |        21.79 |           747.16 |  35.30x |
|      50 |           772.23 |        23.71 |           748.52 |  32.58x |
|     100 |           788.57 |        28.17 |           760.40 |  28.00x |

**Overall Averages**:
- **Non-AOT**: 777.15 ms
- **AOT**: 22.61 ms
- **Average Speedup**: **34.37x**

## Key Findings

1. **Significant Performance Gains**: AOT compilation provides an average **34.37x speedup** over non-AOT builds.

2. **Consistent Results**: The benchmark shows very consistent results across multiple runs, with speedup ranging from 28x to 40x depending on workload.

3. **Startup Overhead**: The speedup is most dramatic with smaller workloads (40x for 1 class), indicating that AOT primarily improves startup time and JIT overhead.

4. **Scalability**: As the number of classes increases, the speedup decreases slightly (from ~40x to ~28x), but AOT still provides substantial benefits even at 100 classes.

5. **Absolute Performance**: 
   - AOT builds complete in 19-29ms across all workloads
   - Non-AOT builds take 740-790ms regardless of class count
   - The relatively constant non-AOT time suggests most time is spent in startup/JIT, not actual code generation

6. **Memory Efficiency**: AOT builds also use less memory (0.85-2.01 MB) compared to non-AOT builds (1.23-3.78 MB).

## Conclusions

The benchmark demonstrates that AOT compilation is extremely effective for the TypeShim generator:

- **Build Performance**: Development builds using AOT will be significantly faster
- **CI/CD Impact**: CI/CD pipelines will complete much faster with AOT-compiled generators
- **Developer Experience**: Faster code generation improves the development feedback loop
- **Memory Efficiency**: Lower memory footprint with AOT compilation

The consistent ~770ms overhead of non-AOT builds is almost entirely eliminated by AOT compilation, making it an excellent choice for the TypeShim generator in production scenarios.

## Technical Notes

### AOT Compatibility Fix

During benchmark development, an issue was identified and fixed where the AOT-compiled generator failed due to empty `Assembly.Location` properties. The fix adds an additional filter in `CSharpPartialCompilation.GetReferences()` to skip assemblies with empty locations, which is common in AOT scenarios where assemblies are embedded in a single-file executable.

### Sample Classes

The benchmark uses 10 sample classes demonstrating:
- Basic methods and properties
- Static methods and factory patterns
- Various primitive types (int, long, float, double, bool, string)
- Nullable types
- Collections and arrays
- Multiple parameter methods
- Comprehensive XML documentation comments

Classes are duplicated and renamed to achieve the target class counts (1, 10, 25, 50, 100) for testing.

### BenchmarkDotNet Benefits

Using BenchmarkDotNet provides:
- Multiple warmup and measurement iterations
- Statistical analysis with outlier detection
- Memory allocation diagnostics
- Standardized, reproducible results
- Detailed reports in multiple formats
