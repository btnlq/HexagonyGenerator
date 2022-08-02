namespace HexagonyGenerator.Bytecode;

class Assignment : IAction
{
    public readonly Variable Dest;
    public readonly ISymbol Left;
    public readonly BinOp Operator;
    public readonly ISymbol? Right;

    public Assignment(Variable dest, ISymbol from)
    {
        Dest = dest;
        Left = from;
    }

    public Assignment(Variable dest, ISymbol left, BinOp @operator, ISymbol right)
    {
        Dest = dest;
        Left = left;
        Operator = @operator;
        Right = right;
    }

    public void ApplyTo(IMemory memory)
    {
        if (Right == null)
            memory.Set(Dest, Left);
        else
            memory.Set(Dest, Left, Operator, Right);
    }
}
