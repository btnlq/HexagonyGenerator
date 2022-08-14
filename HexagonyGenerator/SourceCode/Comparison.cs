namespace HexagonyGenerator.SourceCode;

class Comparison
{
    public readonly IArithmeticExpression Left;
    public readonly IArithmeticExpression Right;
    public readonly ComparisonOp Op;

    public Comparison(IArithmeticExpression left, ComparisonOp op, IArithmeticExpression right)
    {
        Left = left;
        Right = right;
        Op = op;
    }

    public SimplifiedComparison Simplify(SimpleActionList actions, bool forceGoodOp) => SimplifiedComparison.Build(this, actions, forceGoodOp);
}

[System.Flags]
enum ComparisonOp
{
    False = 0,
    Lt = 1,
    Eq = 2,
    Gt = 4,
    Le = Lt | Eq,
    Ge = Gt | Eq,
    Ne = Lt | Gt,
    True = Lt | Eq | Gt,
}

static class ComparisonOpEx
{
    public static bool Has(this ComparisonOp op, int sign) => ((int)op + 1 & (1 << sign + 1)) > 0;
    public static ComparisonOp Reverse(this ComparisonOp op) =>
        (ComparisonOp)(((int)op & 1) << 2 | (int)op >> 2 | (int)op & 2);
}
