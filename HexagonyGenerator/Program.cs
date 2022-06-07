global using System.Collections.Generic;
global using System.Linq;

namespace HexagonyGenerator;

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
