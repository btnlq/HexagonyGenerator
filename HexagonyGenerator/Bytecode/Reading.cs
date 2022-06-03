namespace HexagonyGenerator.Bytecode;

class Reading : ISymbol
{
    public readonly VariableType Type;

    public Reading(VariableType type)
    {
        Type = type;
    }
}
