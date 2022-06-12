namespace HexagonyGenerator;

using System.IO;

enum RunTarget
{
    /// <summary>Run internal bytecode representation (useful for debugging).</summary>
    Bytecode,
    /// <summary>Run internal bytecode representation and reorder procedures by number of calls.</summary>
    BytecodeToOptimizeSpeed,
    /// <summary>Run generated hexagony program.</summary>
    Hexagony
}

/// <summary>
/// Generates and runs hexagony program.
/// </summary>
class Runner
{
    private readonly Bytecode.Program _bytecode;

    private Hexagony.Hexagon? _hexagon;
    private Hexagony.Hexagon Hexagon => _hexagon ??=
        Hexagony.PrimitiveGenerator.Generate(_bytecode) ??
        Hexagony.Generator.Generate(_bytecode);

    /// <param name="codeFilePath">The path to the source code.</param>
    public Runner(string codeFilePath)
    {
        var code = File.ReadAllText(codeFilePath);
        System.Console.WriteLine($"Compiling: {codeFilePath}");
        _bytecode = SourceCode.Compiler.Compiler.Compile(code);
    }

    /// <summary>
    /// Generate hexagony program.
    /// </summary>
    /// <param name="outputFilePath">The file to write generated hexagony program.</param>
    /// <param name="pretty">Generate pretty hexagony code if true, minified otherwise.</param>
    public void Generate(string outputFilePath, bool pretty)
    {
        System.Console.WriteLine($"Generating {(pretty ? "pretty" : "minified")} hexagony: {outputFilePath}");
        var hexagony = Hexagon.ToText(pretty);
        Hexagon.Stats(out int bytes, out int chars, out int operators);
        System.Console.WriteLine($"Side length: {Hexagon.Size + 1:N0}, bytes: {bytes:N0}, chars: {chars:N0}, operators: {operators:N0}");
        File.WriteAllText(outputFilePath, hexagony);
    }

    /// <summary>
    /// Write text representation of the bytecode to the file.
    /// </summary>
    /// <param name="outputFilePath">The file to write the bytecode.</param>
    public void WriteBytecode(string outputFilePath)
    {
        System.Console.WriteLine($"Generating bytecode: {outputFilePath}");
        var text = Bytecode.Formatter.Formatter.Format(_bytecode);
        File.WriteAllText(outputFilePath, text);
    }

    /// <summary>
    /// Run hexagony program.
    /// </summary>
    /// <param name="runTarget">Run internal bytecode or generated hexagony.</param>
    /// <param name="outputFilePath">The file to write program output.</param>
    /// <param name="inputFilePath">(optional) The file with input.
    /// The default argument separator is a newline (each line of the file is interpreted as an argument).
    /// If the file starts with an empty line, the argument separator is an empty line (i.e. at least two newlines).
    /// Input is considered empty if no file is specified.</param>
    /// <param name="compareFilePath">>(optional) The file with expected output.
    /// Runner compares the file content with `outputFilePath` content.</param>
    public void Run(RunTarget runTarget, string outputFilePath,
        string? inputFilePath = null,
        string? compareFilePath = null)
    {
        var reader = GetReader(inputFilePath);

        System.Console.WriteLine(runTarget == RunTarget.Hexagony ? "Executing hexagony" : "Executing bytecode");
        bool optimize = runTarget == RunTarget.BytecodeToOptimizeSpeed;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        string output = runTarget == RunTarget.Hexagony ?
            Hexagony.Interpreter.Executor.Execute(Hexagon, reader, out int executed) :
            Bytecode.Interpreter.Executor.Execute(_bytecode, reader, optimize, out executed);
        stopwatch.Stop();
        System.Console.WriteLine($"Execution time: {stopwatch.Elapsed}, instructions executed: {executed:N0}");

        File.WriteAllText(outputFilePath, output);
        if (compareFilePath != null)
            Compare(outputFilePath, compareFilePath);

        if (optimize)
            _hexagon = null;
    }

    private static Hexagony.Interpreter.Reader GetReader(string? inputFilePath)
    {
        if (inputFilePath == null)
            return new(string.Empty);

        var lines = File.ReadAllLines(inputFilePath);

        if (lines.Length == 0)
            return new(string.Empty);

        var sb = new System.Text.StringBuilder();

        if (lines[0].Length != 0) // single line mode
        {
            foreach (var line in lines)
            {
                sb.Append(line);
                sb.Append('\0');
            }
        }
        else // multiline mode
        {
            bool hasData = false;
            foreach (var line in lines)
            {
                if (line.Length == 0)
                {
                    if (hasData)
                    {
                        sb.Append('\0');
                        hasData = false;
                    }
                }
                else
                {
                    if (hasData)
                        sb.Append('\n');
                    else
                        hasData = true;
                    sb.Append(line);
                }
            }
            if (hasData)
                sb.Append('\0');
        }

        return new(sb.ToString());
    }

    private static void Compare(string actualOutputFilePath, string expectedOutputFilePath)
    {
        var actual = File.ReadAllLines(actualOutputFilePath);
        var expected = File.ReadAllLines(expectedOutputFilePath);

        int linesCount = System.Math.Max(actual.Length, expected.Length);

        for (int i = 0; i < linesCount; i++)
        {
            var actualLine = i < actual.Length ? actual[i].TrimEnd() : string.Empty;
            var expectedLine = i < expected.Length ? expected[i].TrimEnd() : string.Empty;

            if (actualLine != expectedLine)
            {
                System.Console.WriteLine($"# Actual and expected outputs differ on line {i + 1}:");
                System.Console.WriteLine($"#   Actual: " + actualLine);
                System.Console.WriteLine($"# Expected: " + expectedLine);
                return;
            }
        }

        System.Console.WriteLine("Actual and expected outputs are identical");
    }
}
