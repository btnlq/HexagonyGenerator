namespace HexagonyGenerator;

using System.IO;

/// <summary>
/// Generates and runs hexagony program.
/// A wrapper for Runner class: uses predefined file names.
/// </summary>
class Test
{
    private readonly string _testDir;
    private readonly Runner _runner;

    private string FilePath(string path) => Path.Combine(_testDir, path);

    /// <summary>
    /// Initializes a new instance of `Test` class with source code from file "code.txt".
    /// </summary>
    /// <param name="testDir">The directory where files are read and written.</param>
    public Test(string testDir)
    {
        _testDir = testDir;
        _runner = new Runner(FilePath("code.txt"));
    }

    /// <summary>Generate hexagony program with minified layout to file "hexagony.txt".</summary>
    public Test GenerateMinified()
    {
        _runner.Generate(FilePath("hexagony.txt"), false);
        return this;
    }

    /// <summary>Generate hexagony program with pretty layout to file "hexagony_pretty.txt".</summary>
    public Test GeneratePretty()
    {
        _runner.Generate(FilePath("hexagony_pretty.txt"), true);
        return this;
    }

    private static string? IfExists(string path) => File.Exists(path) ? path : null;

    /// <summary>
    /// Run the hexagony program:
    /// - Uses file "input.txt" as input if it exists (see `Runner.Run` for file format).
    /// - Saves program output to file "output_actual.txt".
    /// - Compares it with file "output_expected.txt".
    /// </summary>
    /// <param name="target">Target to run.</param>
    public Test Run(RunTarget target)
    {
        string? inputFilePath = IfExists(FilePath("input.txt"));
        string outputFilePath = FilePath("output_actual.txt");
        string? compareFilePath = IfExists(FilePath("output_expected.txt"));
        _runner.Run(target, outputFilePath, inputFilePath, compareFilePath);
        return this;
    }
}
