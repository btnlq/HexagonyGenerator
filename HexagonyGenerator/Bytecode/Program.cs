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

    public void OrderByDescending(int[] keys)
    {
        Procedures.Sort((p1, p2) => keys[p2.Index].CompareTo(keys[p1.Index]));
        int index = 0;
        foreach (var proc in Procedures)
            proc.Index = index++;
    }
}
