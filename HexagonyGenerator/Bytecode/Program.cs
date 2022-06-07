namespace HexagonyGenerator.Bytecode;

class Program
{
    public readonly List<Procedure> Procedures;
    public readonly Procedure Start;

    public Program(List<Procedure> procedures)
    {
        Procedures = procedures;
        Start = procedures[0];
    }
}
