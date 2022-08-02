namespace HexagonyGenerator.Bytecode;

class Reading : IAction
{
    public readonly VariableType Type;

    public Reading(VariableType type)
    {
        Type = type;
    }

    public void ApplyTo(IMemory memory) => memory.Read(Type);
}
