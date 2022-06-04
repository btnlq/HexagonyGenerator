global using System.Collections.Generic;
global using System.Linq;

namespace HexagonyGenerator;

class Program
{
    static void Main()
    {
        // Test("Example").GenerateMinified().Run(RunTarget.Hexagony);
        foreach (var test in AllTests())
            test.Run(RunTarget.Hexagony);
    }

    private static readonly string _testRoot = System.IO.Path.GetFullPath("../../../Tests");

    private static Test Test(string directory) => new(System.IO.Path.Combine(_testRoot, directory));

    private static IEnumerable<Test> AllTests()
    {
        foreach (var directory in System.IO.Directory.GetDirectories(_testRoot))
            yield return new Test(directory);
    }
}

class UnexpectedDefaultException : System.Exception
{
	public UnexpectedDefaultException() : base("Unreachable code") { }
}
