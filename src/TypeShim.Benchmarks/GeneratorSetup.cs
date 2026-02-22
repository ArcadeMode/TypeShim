using System.Reflection;

namespace TypeShim.Benchmarks;

/// <summary>
/// Manages generator build paths and setup for benchmarks
/// </summary>
public class GeneratorSetup
{
    public string ProjectRoot { get; }
    public string BenchmarkProjectSourceDir { get; }
    public string GeneratorBuildsDir { get; }
    public string NonAotGeneratorPath { get; }
    public string AotGeneratorPath { get; }
    public string SampleClassesDir { get; }
    public string TargetingPackRefDir { get; }

    public GeneratorSetup()
    {
        // Try to get the generator builds directory from environment variable first
        // (This is set by Program.cs before running benchmarks)
        string? generatorBuildsEnvDir = Environment.GetEnvironmentVariable("TYPESHIM_GENERATOR_BUILDS_DIR");
        
        if (!string.IsNullOrEmpty(generatorBuildsEnvDir) && Directory.Exists(generatorBuildsEnvDir))
        {
            // Use the pre-configured directory
            GeneratorBuildsDir = generatorBuildsEnvDir;
            BenchmarkProjectSourceDir = Path.GetFullPath(Path.Combine(GeneratorBuildsDir, ".."));
            ProjectRoot = Path.GetFullPath(Path.Combine(BenchmarkProjectSourceDir, "../.."));
        }
        else
        {
            // Fall back to calculating from assembly location
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string binDir = Path.GetDirectoryName(assemblyLocation)!;
            
            // Navigate from bin/Release/net10.0 up to the project directory
            BenchmarkProjectSourceDir = Path.GetFullPath(Path.Combine(binDir, "../../.."));
            ProjectRoot = Path.GetFullPath(Path.Combine(BenchmarkProjectSourceDir, "../.."));
            GeneratorBuildsDir = Path.Combine(BenchmarkProjectSourceDir, "GeneratorBuilds");
        }

        var filePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "TargetingPackRefDir.txt"));

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Required test configuration file was not found: '{filePath}'. Create it with a single line pointing to a valid directory.");
        }

        var content = File.ReadAllText(filePath).Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException(
                $"Test configuration file '{filePath}' is empty. It must contain a directory path.");
        }

        var dir = Path.GetFullPath(content);

        if (!Directory.Exists(dir))
        {
            throw new DirectoryNotFoundException(
                $"Targeting pack reference directory from '{filePath}' does not exist: '{dir}'.");
        }

        TargetingPackRefDir = dir;

        SampleClassesDir = Path.Combine(BenchmarkProjectSourceDir, "SampleClasses");
        NonAotGeneratorPath = GetGeneratorPath("NonAOT");
        AotGeneratorPath = GetGeneratorPath("AOT");
    }

    private string GetGeneratorPath(string buildType)
    {
        string generatorDir = Path.Combine(GeneratorBuildsDir, buildType);
        
        // On Windows, look for .exe, on Linux/Mac look for the executable without extension
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(generatorDir, "TypeShim.Generator.exe");
        }
        else
        {
            return Path.Combine(generatorDir, "TypeShim.Generator");
        }
    }

    public void Validate()
    {
        if (!Directory.Exists(GeneratorBuildsDir))
        {
            throw new InvalidOperationException(
                $"Generator builds not found at {GeneratorBuildsDir}. " +
                "Please run the build script first:\n" +
                "  Linux/Mac: ./Scripts/build-generators.sh\n" +
                "  Windows:   .\\Scripts\\build-generators.ps1");
        }

        if (!File.Exists(NonAotGeneratorPath))
        {
            throw new FileNotFoundException($"Non-AOT generator not found at {NonAotGeneratorPath}");
        }

        if (!File.Exists(AotGeneratorPath))
        {
            throw new FileNotFoundException($"AOT generator not found at {AotGeneratorPath}");
        }

        if (!Directory.Exists(SampleClassesDir))
        {
            throw new DirectoryNotFoundException($"Sample classes directory not found at {SampleClassesDir}");
        }
    }

    public string[] GetSampleClassFiles()
    {
        string[] files = Directory.GetFiles(SampleClassesDir, "*.cs");
        if (files.Length == 0)
        {
            throw new InvalidOperationException($"No .cs files found in {SampleClassesDir}");
        }
        return files;
    }

    public List<string> GenerateClassFiles(int classCount, string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        // Read the base file which contains 10 sample classes
        string baseContent = File.ReadAllText(GetSampleClassFiles()[0]);
        
        var generatedFiles = new List<string>();
        int classesPerFile = 10;
        int filesNeeded = (classCount + classesPerFile - 1) / classesPerFile; // Ceiling division
        
        for (int fileIndex = 0; fileIndex < filesNeeded; fileIndex++)
        {
            string content = baseContent;
            
            // Replace class names to create unique classes
            if (fileIndex > 0)
            {
                for (int classNum = 1; classNum <= classesPerFile; classNum++)
                {
                    string oldClassName = $"SampleClass{classNum:D2}";
                    string newClassName = $"SampleClass{fileIndex}_{classNum:D2}";
                    content = content.Replace($"class {oldClassName}", $"class {newClassName}");
                    content = content.Replace($"static {oldClassName} Create()", $"static {newClassName} Create()");
                    content = content.Replace($"return new {oldClassName}", $"return new {newClassName}");
                    content = content.Replace($"private {oldClassName}(", $"private {newClassName}(");
                }
            }
            
            string filePath = Path.Combine(outputDir, $"SampleClasses_{fileIndex}.cs");
            File.WriteAllText(filePath, content);
            generatedFiles.Add(filePath);
        }

        return generatedFiles;
    }
}
