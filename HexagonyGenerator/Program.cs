global using System.Collections.Generic;
global using System.Linq;

namespace HexagonyGenerator;

static class Configuration
{
    /// <summary>
    /// Replace commands like "put(10)" with "put(266)".
    /// It increases the number of bytes but may help to generate a smaller grid.
    /// </summary>
    public static bool OptimizePut => false;
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
                .Run(RunTarget.BytecodeToOptimize)
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
