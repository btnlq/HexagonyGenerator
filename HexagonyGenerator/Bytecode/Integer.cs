namespace HexagonyGenerator.Bytecode;

class Integer : ISymbol
{
    public readonly Value Value;

    public Integer(Value value)
    {
        Value = value;
    }
}
