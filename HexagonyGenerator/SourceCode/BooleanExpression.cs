namespace HexagonyGenerator.SourceCode;

using Bytecode;

interface IBooleanExpression
{
    IEnumerable<IStatement> ToStatement(Block? trueBlock, Block? falseBlock);
}

static class BooleanExpression
{
    public static IBooleanExpression Comparison(Comparison comparison)
        => new ComparisonExpression(comparison);
    public static IBooleanExpression Not(IBooleanExpression expression)
        => new NotExpression(expression);
    public static IBooleanExpression And(IBooleanExpression left, IBooleanExpression right)
        => new AndExpression(left, right);
    public static IBooleanExpression Or(IBooleanExpression left, IBooleanExpression right)
        => Not(And(Not(left), Not(right)));

    // if (Left && Right) { trueBlock; } else { falseBlock; }
    private class AndExpression : IBooleanExpression
    {
        public readonly IBooleanExpression Left;
        public readonly IBooleanExpression Right;

        public AndExpression(IBooleanExpression left, IBooleanExpression right)
        {
            Left = left;
            Right = right;
        }

        public IEnumerable<IStatement> ToStatement(Block? trueBlock, Block? falseBlock)
        {
            if (falseBlock == null)
            {
                // if (Left) if (Right) { trueBlock; }
                return Left.ToStatement(new Block { Right.ToStatement(trueBlock, null) }, null);
            }
            else
            {
                // block {
                //   if (Left) if (Right) { trueBlock; break block; }
                //   falseBlock;
                // }

                var block = new Block();

                var @goto = new Goto(GotoType.Break, block);
                if (trueBlock != null)
                    trueBlock.Add(@goto);
                else
                    trueBlock = new Block { @goto };

                block.Add(Left.ToStatement(new Block { Right.ToStatement(trueBlock, null) }, null));
                block.Add(falseBlock);
                return new BlockStatement(block);
            }
        }
    }

    // if (!Expression) { trueBlock; } else { falseBlock; }
    private class NotExpression : IBooleanExpression
    {
        public readonly IBooleanExpression Expression;

        public NotExpression(IBooleanExpression expression)
        {
            Expression = expression;
        }

        public IEnumerable<IStatement> ToStatement(Block? trueBlock, Block? falseBlock)
        {
            // if (Expression) { falseBlock; } else { trueBlock; }
            return Expression.ToStatement(falseBlock, trueBlock);
        }
    }

    private class ComparisonExpression : IBooleanExpression
    {
        public readonly Comparison Comparison;

        public ComparisonExpression(Comparison comparison)
        {
            Comparison = comparison;
        }

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

        public IEnumerable<IStatement> ToStatement(Block? trueBlock, Block? falseBlock)
        {
            SimpleActionList actions = new();
            var left = Comparison.Left.ToSymbol(actions);
            var right = Comparison.Right.ToSymbol(actions);

            if (left is Integer leftValue)
            {
                if (right is Integer rightValue)
                {
                    int sign = leftValue.Value.CompareTo(rightValue.Value);
                    bool result = Comparison.Op.Has(sign);
                    return (result ? trueBlock : falseBlock) ?? Enumerable.Empty<IStatement>();
                }
                else if (right is Variable rightVariable)
                {
                    if (IsZeroComparison(Comparison.Op.Reverse(), leftValue, out var op))
                        return actions.Statements.Append(new Conditional(rightVariable, op, trueBlock, falseBlock));
                }
            }
            else if (left is Variable leftVariable)
            {
                if (right is Integer rightValue)
                {
                    if (IsZeroComparison(Comparison.Op, rightValue, out var op))
                        return actions.Statements.Append(new Conditional(leftVariable, op, trueBlock, falseBlock));
                }
            }

            var variable = Compiler.VariableAllocator.New();
            return actions.Statements
                .Append(new Assignment(variable, left, BinOp.Sub, right).AsStatement())
                .Append(new Conditional(variable, Comparison.Op, trueBlock, falseBlock));
        }
    }
}
