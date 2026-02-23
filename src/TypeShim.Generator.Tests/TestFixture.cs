namespace TypeShim.Generator.Tests;

using System;
using System.IO;
using NUnit.Framework;

[SetUpFixture]
public sealed class TestFixture
{
    public static string TargetingPackRefDir { get; private set; } = string.Empty;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
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
    }
}
