namespace HexagonyGenerator.Bytecode;

class Reading : ISymbol, IAction
{
    public readonly VariableType Type;

    public Reading(VariableType type)
    {
        Type = type;
    }

    public void ApplyTo(IMemory memory) => memory.Read(Type);
}
