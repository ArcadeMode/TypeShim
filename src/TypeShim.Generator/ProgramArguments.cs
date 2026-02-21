using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace TypeShim.Generator;

internal sealed class ProgramArguments
{
    internal CSharpFileInfo[] CsFileInfos { get; private init; }
    internal string CsOutputDir { get; private init; }
    internal string TsOutputFilePath { get; private init; }
    internal string RuntimePackRefDir {  get; private init; }

    private ProgramArguments(CSharpFileInfo[] csFileInfos, string csOutputDir, string tsOutputFilePath, string runtimePackRefDir)
    {
        CsFileInfos = csFileInfos;
        CsOutputDir = csOutputDir;
        TsOutputFilePath = tsOutputFilePath;
        RuntimePackRefDir = runtimePackRefDir;
    }

    internal static ProgramArguments Parse(string[] args)
    {
        if (args.Length != 4)
        {
            Console.Error.WriteLine("TypeShim usage: <csFilePaths> <csOutputDir> <tsOutputFilePath>");
            Environment.Exit(1);
        }

        return new ProgramArguments(ParseCsFilePaths(args[0]), ParseCsOutputDir(args[1]), ParseTsOutputFilePath(args[2]), args[3]);
    }

    private static CSharpFileInfo[] ParseCsFilePaths(string arg)
    {
        string[] csFilePaths = arg.Split(';');
        if (csFilePaths.Length == 0)
        {
            Console.Error.WriteLine("No .cs file paths provided");
            Environment.Exit(1);
        }
        CSharpFileInfo[] fileInfos = new CSharpFileInfo[csFilePaths.Length];
        for (int i = 0; i < csFilePaths.Length; i++)
        {
            string csFilePath = csFilePaths[i];

            if (!File.Exists(csFilePath))
            {
                Console.Error.WriteLine($"Invalid .cs file path provided '{csFilePath}'");
                Environment.Exit(1);
            }

            using FileStream fs = new(csFilePath, FileMode.Open, FileAccess.Read);
            fileInfos[i] = CSharpFileInfo.Create(CSharpSyntaxTree.ParseText(SourceText.From(fs)));
        }
        return fileInfos;
    }

    private static string ParseCsOutputDir(string arg)
    {
        string csOutputDir = arg;
        try
        {
            csOutputDir = Path.GetFullPath(csOutputDir);
            if (!Directory.Exists(csOutputDir))
            {
                Directory.CreateDirectory(csOutputDir);
            }
        }
        catch (Exception)
        {
            Console.Error.WriteLine($"Invalid output directory provided '{csOutputDir}'");
            Environment.Exit(1);
        }
        return csOutputDir;
    }

    private static string ParseTsOutputFilePath(string arg)
    {
        string tsOutputFilePath = arg;
        if (string.IsNullOrWhiteSpace(tsOutputFilePath))
        {
            Console.Error.WriteLine($"Invalid TypeScript output file path provided '{tsOutputFilePath}'");
            Environment.Exit(1);
        }
        try
        {
            tsOutputFilePath = Path.GetFullPath(tsOutputFilePath);
            string tsOutputFileDir = Path.GetDirectoryName(tsOutputFilePath) ?? throw new InvalidOperationException($"Provided path {tsOutputFilePath} has no directory");
            if (!Directory.Exists(tsOutputFileDir))
            {
                Directory.CreateDirectory(tsOutputFileDir);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Invalid output directory provided '{tsOutputFilePath}'. Error {ex.Message}");
            Environment.Exit(1);
        }
        return tsOutputFilePath;
    }
}
