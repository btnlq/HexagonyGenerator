namespace HexagonyGenerator.Bytecode;

enum VariableType { Int, Byte }

class Writing : IAction
{
    public readonly VariableType Type;
    public readonly ISymbol Symbol;

    public Writing(VariableType type, ISymbol symbol)
    {
        Type = type;
        Symbol = symbol;
    }

    public void ApplyTo(IMemory memory)
    {
        switch (Type)
        {
            case VariableType.Int:
                memory.WriteInt(Symbol); break;
            case VariableType.Byte:
                memory.WriteByte(Symbol); break;
            default:
                throw new UnexpectedDefaultException();
        }
    }
}
