namespace HexagonyGenerator.SourceCode;

using Bytecode;

interface IArithmeticExpression
{
    ISymbol ToSymbol(SimpleActionList actions);
    void AssignTo(Variable variable, SimpleActionList actions);
    Integer? AsInteger => null;
}

static class ArithmeticExpression
{
    public static IArithmeticExpression Create(ISymbol symbol) => new SymbolExpression(symbol);
    public static IArithmeticExpression Create(IArithmeticExpression left, BinOp op, IArithmeticExpression right)
    {
        Integer? leftInteger, rightInteger;
        if ((leftInteger = left.AsInteger) != null && (rightInteger = right.AsInteger) != null)
            return new SymbolExpression(new Integer(op.Compute(leftInteger.Value, rightInteger.Value)));
        else
            return new BinaryExpression(left, op, right);
    }
    public static IArithmeticExpression? CreatePower(IArithmeticExpression left, IArithmeticExpression right, out string error)
    {
        Integer? leftInteger, rightInteger;
        if ((leftInteger = left.AsInteger) == null || (rightInteger = right.AsInteger) == null)
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
    public static IArithmeticExpression Create(Comparison comparison, IArithmeticExpression trueValue, IArithmeticExpression falseValue)
        => new ConditionalExpression(comparison, trueValue, falseValue);

    private class BinaryExpression : IArithmeticExpression
    {
        private readonly IArithmeticExpression Left;
        private readonly IArithmeticExpression Right;
        private readonly BinOp Op;

        public BinaryExpression(IArithmeticExpression left, BinOp op, IArithmeticExpression right)
        {
            Left = left;
            Right = right;
            Op = op;
        }

        private static bool IsSmallInt(Integer integer, out int value)
        {
            var bigValue = integer.Value;
            bool isSmall = Value.Abs(bigValue) <= 10;
            value = isSmall ? (int)bigValue : 0;
            return isSmall;
        }

        private ISymbol? TryOptimize(ISymbol leftSymbol, ISymbol rightSymbol)
        {
            if (leftSymbol is Integer leftInteger)
            {
                if (rightSymbol is Integer rightInteger)
                {
                    var value = Op.Compute(leftInteger.Value, rightInteger.Value);
                    return new Integer(value);
                }
                else
                {
                    if (IsSmallInt(leftInteger, out int leftValue))
                    {
                        var symbol = (ModifiableSymbol)rightSymbol;
                        return Op switch
                        {
                            BinOp.Add => Modifier.Add(symbol, leftValue),
                            BinOp.Sub => Modifier.Add(Modifier.Negate(symbol), leftValue),
                            BinOp.Mul => Modifier.Mul(symbol, leftValue),
                            _ => null,
                        };
                    }
                }
            }
            else
            {
                if (rightSymbol is Integer rightInteger)
                {
                    if (IsSmallInt(rightInteger, out int rightValue))
                    {
                        var symbol = (ModifiableSymbol)leftSymbol;
                        return Op switch
                        {
                            BinOp.Add => Modifier.Add(symbol, rightValue),
                            BinOp.Sub => Modifier.Add(symbol, -rightValue),
                            BinOp.Mul => Modifier.Mul(symbol, rightValue),
                            BinOp.Div => Modifier.Div(symbol, rightValue),
                            BinOp.Mod => Modifier.Mod(symbol, rightValue),
                            _ => null,
                        };
                    }
                }
            }

            return null;
        }

        public ISymbol ToSymbol(SimpleActionList actions)
        {
            var left = Left.ToSymbol(actions);
            var right = Right.ToSymbol(actions);

            var symbol = TryOptimize(left, right);
            if (symbol != null)
                return symbol;

            var variable = Compiler.VariableAllocator.New();
            actions.AddAssignment(new(variable, left, Op, right));
            return new VariableSymbol(variable);
        }

        public void AssignTo(Variable variable, SimpleActionList actions)
        {
            var left = Left.ToSymbol(actions);
            var right = Right.ToSymbol(actions);

            var symbol = TryOptimize(left, right);
            actions.AddAssignment(symbol != null ?
                new(variable, symbol) :
                new(variable, left, Op, right)
            );
        }
    }

    private class SymbolExpression : IArithmeticExpression
    {
        private readonly ISymbol Symbol;

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

        Integer? IArithmeticExpression.AsInteger => Symbol as Integer;
    }

    private class ConditionalExpression : IArithmeticExpression
    {
        private readonly Comparison Comparison;
        private readonly IArithmeticExpression TrueValue;
        private readonly IArithmeticExpression FalseValue;

        public ConditionalExpression(Comparison comparison, IArithmeticExpression trueValue, IArithmeticExpression falseValue)
        {
            Comparison = comparison;
            TrueValue = trueValue;
            FalseValue = falseValue;
        }

        public void AssignTo(Variable variable, SimpleActionList actions)
        {
            var comparison = Comparison.Simplify(actions, true);
            if (comparison.Symbol == null)
            {
                (comparison.Op == ComparisonOp.True ? TrueValue : FalseValue).AssignTo(variable, actions);
                return;
            }

            var trueValue = TrueValue.ToSymbol(actions);
            var falseValue = FalseValue.ToSymbol(actions);
            if (comparison.Op == ComparisonOp.Le)
                (trueValue, falseValue) = (falseValue, trueValue);

            actions.AddConditionalAssignment(new(variable, comparison.Symbol, trueValue, falseValue));
        }

        public ISymbol ToSymbol(SimpleActionList actions)
        {
            var variable = Compiler.VariableAllocator.New();
            AssignTo(variable, actions);
            return new VariableSymbol(variable);
        }
    }
}
