global using System.Collections.Generic;
global using System.Linq;
global using Math = System.Math;

namespace HexagonyGenerator;

static class Configuration
{
    /// <summary>
    /// `Chars` and `ForceChars` options increase the number of bytes but may help to generate a smaller grid.
    /// See `ValueEncoding` enum for more details.
    /// </summary>
    public static ValueEncoding ValueEncoding => ValueEncoding.Chars;
}

class Program
{
    static void Main()
    {
        var testRoot = System.IO.Path.GetFullPath("../../../../Tests");
        foreach (var directory in System.IO.Directory.GetDirectories(testRoot /*, "Fibonacci" */))
        {
            new Test(directory)
                //.Run(RunTarget.Bytecode)
                //.Run(RunTarget.BytecodeToOptimizeSpeed)
                .Run(RunTarget.Hexagony)
                //.GenerateBytecode()
                .GenerateMinified()
                //.GeneratePretty()
                ;
            System.Console.WriteLine();
        }
    }
}

class UnexpectedDefaultException : System.Exception
{
    public UnexpectedDefaultException() : base("Unreachable code") { }
}
