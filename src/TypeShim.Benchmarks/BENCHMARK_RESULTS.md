# TypeShim Generator Benchmark Results

## Overview

This document contains benchmark results comparing AOT (Ahead-of-Time) and non-AOT builds of TypeShim.Generator across different workloads.

## Test Environment

- **Platform**: Linux (GitHub Actions runner)
- **.NET Version**: 10.0.102
- **Test Date**: February 18, 2026
- **Sample Classes**: 10 diverse classes with various methods, properties, constructors, types, and XML documentation

## Benchmark Configuration

The benchmark tests code generation performance with:
- **Class Counts**: 1, 10, 25, 50, and 100 classes
- **Build Types**: AOT and non-AOT
- **Measurements**: Code generation duration in milliseconds

## Results Summary

### Run 1
| Classes | Non-AOT (ms) | AOT (ms) | Improvement | Speedup |
|---------|--------------|----------|-------------|---------|
|       1 |       792.15 |    19.72 |      772.42 |   40.16x |
|      10 |       743.84 |    19.05 |      724.80 |   39.05x |
|      25 |       758.68 |    21.76 |      736.92 |   34.86x |
|      50 |       771.11 |    24.28 |      746.83 |   31.76x |
|     100 |       787.77 |    27.57 |      760.20 |   28.57x |

**Average Non-AOT**: 770.71 ms  
**Average AOT**: 22.48 ms  
**Average Speedup**: **34.29x**

### Run 2
| Classes | Non-AOT (ms) | AOT (ms) | Improvement | Speedup |
|---------|--------------|----------|-------------|---------|
|       1 |       787.62 |    21.44 |      766.18 |   36.73x |
|      10 |       767.23 |    18.58 |      748.65 |   41.29x |
|      25 |       756.82 |    21.91 |      734.92 |   34.55x |
|      50 |       767.71 |    23.36 |      744.35 |   32.87x |
|     100 |       794.12 |    28.23 |      765.89 |   28.13x |

**Average Non-AOT**: 774.70 ms  
**Average AOT**: 22.70 ms  
**Average Speedup**: **34.12x**

### Run 3
| Classes | Non-AOT (ms) | AOT (ms) | Improvement | Speedup |
|---------|--------------|----------|-------------|---------|
|       1 |       793.78 |    19.69 |      774.10 |   40.32x |
|      10 |       783.45 |    19.73 |      763.72 |   39.71x |
|      25 |       791.34 |    21.69 |      769.65 |   36.48x |
|      50 |       777.88 |    23.49 |      754.39 |   33.12x |
|     100 |       783.82 |    28.71 |      755.11 |   27.30x |

**Average Non-AOT**: 786.06 ms  
**Average AOT**: 22.66 ms  
**Average Speedup**: **34.69x**

## Overall Statistics (3 runs)

| Metric | Value |
|--------|-------|
| **Average Non-AOT Duration** | 777.16 ms |
| **Average AOT Duration** | 22.61 ms |
| **Average Speedup** | **34.37x** |
| **Minimum Speedup** | 27.30x (100 classes) |
| **Maximum Speedup** | 41.29x (10 classes) |

## Key Findings

1. **Significant Performance Gains**: AOT compilation provides an average **34.37x speedup** over non-AOT builds.

2. **Consistent Results**: The benchmark shows very consistent results across multiple runs, with speedup ranging from 34.12x to 34.69x.

3. **Startup Overhead**: The speedup is most dramatic with smaller workloads (40x for 1 class), indicating that AOT primarily improves startup time and JIT overhead.

4. **Scalability**: As the number of classes increases, the speedup decreases slightly (from ~40x to ~28x), but AOT still provides substantial benefits even at 100 classes.

5. **Absolute Performance**: 
   - AOT builds complete in 19-29ms across all workloads
   - Non-AOT builds take 740-790ms regardless of class count
   - The relatively constant non-AOT time suggests most time is spent in startup/JIT, not actual code generation

## Conclusions

The benchmark demonstrates that AOT compilation is extremely effective for the TypeShim generator:

- **Build Performance**: Development builds using AOT will be significantly faster
- **CI/CD Impact**: CI/CD pipelines will complete much faster with AOT-compiled generators
- **Developer Experience**: Faster code generation improves the development feedback loop

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
