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

        public IEnumerable<IStatement> ToStatement(Block? trueBlock, Block? falseBlock)
        {
            return new ConditionBuilder(trueBlock, falseBlock).ToStatement(Comparison);
        }

        private struct ConditionBuilder
        {
            private readonly SimpleActionList Actions = new();
            private readonly Block? TrueBlock;
            private readonly Block? FalseBlock;

            public ConditionBuilder(Block? trueBlock, Block? falseBlock)
            {
                TrueBlock = trueBlock;
                FalseBlock = falseBlock;
            }

            private IEnumerable<IStatement> Build(ModifiableSymbol symbol, ComparisonOp op) =>
                Actions.Statements.Append(new Conditional(symbol, op, TrueBlock, FalseBlock));

            private static bool Bad(ComparisonOp op) => op == ComparisonOp.Ge || op == ComparisonOp.Lt;

            private IEnumerable<IStatement>? TryBuild(ModifiableSymbol symbol, ComparisonOp op, Integer integer)
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

                if (symbol.ModifiersCount > 0 && Bad(op))
                {
                    Modifier.Negate(symbol);
                    op = op.Reverse();
                }

                return Build(symbol, op);
            }

            private IEnumerable<IStatement> Build(ISymbol left, ComparisonOp op, ISymbol right)
            {
                if (Bad(op))
                {
                    (left, right) = (right, left);
                    op = op.Reverse();
                }

                var variable = Compiler.VariableAllocator.New();
                Actions.AddAssignment(new Assignment(variable, left, BinOp.Sub, right));
                return Build(new VariableSymbol(variable), op);
            }

            public IEnumerable<IStatement> ToStatement(Comparison comparison)
            {
                var left = comparison.Left.ToSymbol(Actions);
                var right = comparison.Right.ToSymbol(Actions);

                IEnumerable<IStatement>? statements = null;

                if (left is Integer leftValue)
                {
                    if (right is Integer rightValue)
                    {
                        int sign = leftValue.Value.CompareTo(rightValue.Value);
                        bool result = comparison.Op.Has(sign);
                        statements = (result ? TrueBlock : FalseBlock) ?? Enumerable.Empty<IStatement>();
                    }
                    else
                        statements = TryBuild((ModifiableSymbol)right, comparison.Op.Reverse(), leftValue);
                }
                else
                {
                    if (right is Integer rightValue)
                        statements = TryBuild((ModifiableSymbol)left, comparison.Op, rightValue);
                }

                return statements ?? Build(left, comparison.Op, right);
            }
        }
    }
}
