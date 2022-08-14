namespace HexagonyGenerator.SourceCode;
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
            SimpleActionList actions = new();
            var comparison = Comparison.Simplify(actions, false);
            if (comparison.Symbol == null)
                return (comparison.Op == ComparisonOp.True ? trueBlock : falseBlock) ?? Enumerable.Empty<IStatement>();
            return actions.Statements.Append(new Conditional(comparison.Symbol, comparison.Op, trueBlock, falseBlock));
        }
    }
}
