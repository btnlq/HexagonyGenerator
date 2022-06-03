namespace HexagonyGenerator.SourceCode;

class Condition
{
    public readonly IArithmeticExpression Left;
    public readonly IArithmeticExpression Right;
    public readonly ComparisonOp Op;

    public Condition(IArithmeticExpression left, ComparisonOp op, IArithmeticExpression right)
    {
        Left = left;
        Right = right;
        Op = op;
    }
}

enum ComparisonOp
{
    Lt = 1,
    Eq = 2,
    Gt = 4,
    Le = Lt | Eq,
    Ge = Gt | Eq,
    Ne = Lt | Gt,
}

static class ComparisonOpEx
{
    public static bool Has(this ComparisonOp op, int sign) => ((int)op + 1 & (1 << sign + 1)) > 0;
    public static ComparisonOp Reverse(this ComparisonOp op) =>
        (ComparisonOp)(((int)op & 1) << 2 | (int)op >> 2 | (int)op & 2);
}
