namespace HexagonyGenerator.SourceCode;

using Bytecode;

class SimplifiedComparison
{
    public readonly ModifiableSymbol? Symbol;
    public readonly ComparisonOp Op;

    private SimplifiedComparison(ModifiableSymbol? symbol, ComparisonOp op)
    {
        Symbol = symbol;
        Op = op;
    }

    private static bool Bad(ComparisonOp op) => op == ComparisonOp.Ge || op == ComparisonOp.Lt;

    private static SimplifiedComparison? TryBuild(ModifiableSymbol symbol, ComparisonOp op, Integer integer, bool forceGoodOp)
    {
        if (Value.Abs(integer.Value) > 2)
            return null;

        int value = (int)integer.Value;

        if (value > 0)
        {
            if (op == ComparisonOp.Ge || op == ComparisonOp.Lt)
            {
                value--;
                op ^= ComparisonOp.Eq;
            }
        }
        else if (value < 0)
        {
            if (op == ComparisonOp.Gt || op == ComparisonOp.Le)
            {
                value++;
                op ^= ComparisonOp.Eq;
            }
        }

        if (value == 1)
            Modifier.Decrement(symbol);
        else if (value == -1)
            Modifier.Increment(symbol);
        else if (value != 0)
            return null;

        if (Bad(op) && (forceGoodOp || symbol.ModifiersCount > 0))
        {
            Modifier.Negate(symbol);
            op = op.Reverse();
        }

        return new(symbol, op);
    }

    public static SimplifiedComparison Build(Comparison comparison, SimpleActionList actions, bool forceGoodOp)
    {
        var left = comparison.Left.ToSymbol(actions);
        var right = comparison.Right.ToSymbol(actions);
        var op = comparison.Op;

        SimplifiedComparison? result = null;

        if (left is Integer leftValue)
        {
            if (right is Integer rightValue)
            {
                int sign = leftValue.Value.CompareTo(rightValue.Value);
                return new(null, op.Has(sign) ? ComparisonOp.True : ComparisonOp.False);
            }
            else
                result = TryBuild((ModifiableSymbol)right, op.Reverse(), leftValue, forceGoodOp);
        }
        else
        {
            if (right is Integer rightValue)
                result = TryBuild((ModifiableSymbol)left, op, rightValue, forceGoodOp);
        }

        if (result != null)
            return result;

        if (Bad(op))
        {
            (left, right) = (right, left);
            op = op.Reverse();
        }

        var variable = Compiler.VariableAllocator.New();
        actions.AddAssignment(new(variable, left, BinOp.Sub, right));
        return new(new VariableSymbol(variable), op);
    }
}
