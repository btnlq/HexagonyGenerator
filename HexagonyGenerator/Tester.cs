namespace HexagonyGenerator;

using System.IO;

/// <summary>
/// A wrapper for Runner class.
/// Uses predefined file names.
/// </summary>
class Tester
{
    private readonly string _testDir;

    /// <param name="testDir">The path to directory with test directories.</param>
    public Tester(string testDir) => _testDir = testDir;

    /// <summary>
    /// Test all test directories inside `testDir` directory.
    /// </summary>
    /// <param name="generate">Layouts of hexagony programs to generate.</param>
    /// <param name="targets">Targets to run.</param>
    public void TestAll(Generate generate, params RunTarget[] targets)
    {
        foreach (var directory in Directory.GetDirectories(_testDir))
            Test(directory, generate, targets);
    }

    [System.Flags]
    public enum Generate
    {
        /// <summary>Do not generate hexagony program.</summary>
        None,
        /// <summary>Generate minified layout to file "hexagony.txt".</summary>
        Minified,
        /// <summary>Generate pretty layout to file "hexagony_pretty.txt".</summary>
        Pretty,
        /// <summary>Generate both layouts.</summary>
        Both
    }

    /// <summary>
    /// Loads source code from file "code.txt".
    /// Generates hexagony programs according to the `generate` argument.
    /// Runs the hexagony program according to the `targets` argument:
    /// - Uses file "input.txt" as input if it exists (see `Runner.Run` for file format).
    /// - Saves program output to file "output_actual.txt".
    /// - Compares it with file "output_expected.txt".
    /// </summary>
    /// <param name="directory">The directory to test (relative or absolute path).</param>
    /// <param name="generate">Layouts of hexagony programs to generate.</param>
    /// <param name="targets">Targets to run.</param>
    public void Test(string directory, Generate generate, params RunTarget[] targets)
    {
        if (!Path.IsPathFullyQualified(directory))
            directory = Path.Combine(_testDir, directory);

        string FilePath(string path) => Path.Combine(directory, path);

        var runner = new Runner(FilePath("code.txt"));

        if (generate.HasFlag(Generate.Minified))
            runner.Generate(FilePath("hexagony.txt"), false);
        if (generate.HasFlag(Generate.Pretty))
            runner.Generate(FilePath("hexagony_pretty.txt"), true);

        if (targets.Length > 0)
        {
            string? inputFilePath = IfExists(FilePath("input.txt"));
            string outputFilePath = FilePath("output_actual.txt");
            string? compareFilePath = IfExists(FilePath("output_expected.txt"));
            foreach (var target in targets)
                runner.Run(target, outputFilePath, inputFilePath, compareFilePath);
        }
    }

    private static string? IfExists(string path) => File.Exists(path) ? path : null;
}
