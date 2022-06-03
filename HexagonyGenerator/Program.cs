global using System.Collections.Generic;
global using System.Linq;

namespace HexagonyGenerator;

class Program
{
    static void Main()
    {
        var tester = new Tester(System.IO.Path.GetFullPath("../../../Tests"));
        // tester.Test("Example", Tester.Generate.Minified, RunTarget.Hexagony);
        tester.TestAll(Tester.Generate.None, RunTarget.Hexagony);
    }
}

class UnexpectedDefaultException : System.Exception
{
	public UnexpectedDefaultException() : base("Unreachable code") { }
}
