namespace HexagonyGenerator.SourceCode;

using Bytecode;

static class Desugar
{
    private static bool IsZeroComparison(ComparisonOp op, Integer integer, out ComparisonOp simpleOp)
    {
        if (integer.Value.IsZero)
        {
            simpleOp = op;
            return true;
        }
        else if (Value.Abs(integer.Value).IsOne)
        {
            int value = (int)integer.Value;
            if (value > 0)
            {
                if (op == ComparisonOp.Lt) // x < 1
                {
                    simpleOp = ComparisonOp.Le; // x <= 0
                    return true;
                }
            }
            else
            {
                if (op == ComparisonOp.Gt) // x > -1
                {
                    simpleOp = ComparisonOp.Ge; // x >= 0
                    return true;
                }
            }
        }
        simpleOp = default;
        return false;
    }

    public static IEnumerable<IStatement> Conditional(Condition condition, Block? firstBlock, Block? secondBlock)
    {
        SimpleActionList actions = new();
        var left = condition.Left.ToSymbol(actions);
        var right = condition.Right.ToSymbol(actions);

        if (left is Integer leftValue)
        {
            if (right is Integer rightValue)
            {
                int sign = leftValue.Value.CompareTo(rightValue.Value);
                bool result = condition.Op.Has(sign);
                return (result ? firstBlock : secondBlock) ?? Enumerable.Empty<IStatement>();
            }
            else if (right is Variable rightVariable)
            {
                if (IsZeroComparison(condition.Op.Reverse(), leftValue, out var op))
                    return actions.Statements.Append(new Conditional(rightVariable, op, firstBlock, secondBlock));
            }
        }
        else if (left is Variable leftVariable)
        {
            if (right is Integer rightValue)
            {
                if (IsZeroComparison(condition.Op, rightValue, out var op))
                    return actions.Statements.Append(new Conditional(leftVariable, op, firstBlock, secondBlock));
            }
        }

        var variable = Compiler.VariableAllocator.New();
        return actions.Statements
            .Append(new Assignment(variable, left, BinOp.Sub, right).AsStatement())
            .Append(new Conditional(variable, condition.Op, firstBlock, secondBlock));
    }

    public static IEnumerable<IStatement> For(IEnumerable<IStatement>? initializer, Condition? condition, IEnumerable<IStatement>? iterator, Block block)
    {
        if (condition != null)
        {
            // `if (!condition) break block;`
            var breakBlock = new Block { new Goto(GotoType.Break, block) };
            block.AddToFront(Conditional(condition, null, breakBlock));
        }
        if (iterator != null)
            block.Add(iterator);
        Loop loop = new(block);
        return initializer != null ? initializer.Append(loop) : loop;
    }
}
