namespace HexagonyGenerator.SourceCode;

using Bytecode;

interface IArithmeticExpression
{
    ISymbol ToSymbol(SimpleActionList actions);
    void AssignTo(Variable variable, SimpleActionList actions);
}

static class ArithmeticExpression
{
    public static IArithmeticExpression Create(ISymbol symbol) => new SymbolExpression(symbol);
    public static IArithmeticExpression Create(IArithmeticExpression left, BinOp op, IArithmeticExpression right)
    {
        if (left is SymbolExpression { Symbol: Integer leftInteger } &&
            right is SymbolExpression { Symbol: Integer rightInteger })
            return new SymbolExpression(new Integer(op.Compute(leftInteger.Value, rightInteger.Value)));
        else
            return new BinaryExpression(left, op, right);
    }
    public static IArithmeticExpression? CreatePower(IArithmeticExpression left, IArithmeticExpression right, out string error)
    {
        if (!(left is SymbolExpression { Symbol: Integer leftInteger } &&
            right is SymbolExpression { Symbol: Integer rightInteger }))
        {
            error = "Power operator operands must be constants";
            return null;
        }

        var exponent = rightInteger.Value;
        if (exponent.Sign < 0)
        {
            error = "Exponent must be non-negative";
            return null;
        }

        if (exponent > int.MaxValue)
        {
            error = "Exponent must be less than 2^31";
            return null;
        }

        error = null!;
        return new SymbolExpression(new Integer(Value.Pow(leftInteger.Value, (int)exponent)));
    }

    private class BinaryExpression : IArithmeticExpression
    {
        public IArithmeticExpression Left;
        public IArithmeticExpression Right;
        public BinOp Op;

        public BinaryExpression(IArithmeticExpression left, BinOp op, IArithmeticExpression right)
        {
            Left = left;
            Right = right;
            Op = op;
        }

        public ISymbol ToSymbol(SimpleActionList actions)
        {
            var variable = Compiler.VariableAllocator.New();
            AssignTo(variable, actions);
            return variable;
        }

        public void AssignTo(Variable variable, SimpleActionList actions)
        {
            var left = Left.ToSymbol(actions);
            var right = Right.ToSymbol(actions);
            actions.AddAssignment(new(variable, left, Op, right));
        }
    }

    private class SymbolExpression : IArithmeticExpression
    {
        public ISymbol Symbol;

        public SymbolExpression(ISymbol symbol)
        {
            Symbol = symbol;
        }

        public ISymbol ToSymbol(SimpleActionList actions)
        {
            return Symbol;
        }

        public void AssignTo(Variable variable, SimpleActionList actions)
        {
            actions.AddAssignment(new(variable, Symbol));
        }
    }
}
