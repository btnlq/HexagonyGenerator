namespace HexagonyGenerator.Bytecode;

class Program
{
    public readonly List<Procedure> Procedures;
    public Procedure Start => Procedures[0];

    public Program(List<Procedure> procedures)
    {
        Procedures = procedures;
    }
}
