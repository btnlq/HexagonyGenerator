namespace HexagonyGenerator.Bytecode;

using Command = Hexagony.Command;

enum BinOp
{
    Add = Command.Add,
    Sub = Command.Sub,
    Mul = Command.Mul,
    Div = Command.Div,
    Mod = Command.Mod,
}

static class BinOpEx
{
    public static Value Compute(this BinOp op, Value left, Value right)
    {
        return op switch
        {
            BinOp.Add => left + right,
            BinOp.Sub => left - right,
            BinOp.Mul => left * right,
            BinOp.Div => left.Div(right),
            BinOp.Mod => left.Mod(right),
            _ => throw new UnexpectedDefaultException(),
        };
    }
}
