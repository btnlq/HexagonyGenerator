namespace HexagonyGenerator.SourceCode;

static class Desugar
{
    // `if (!condition) break block;`
    private static IEnumerable<IStatement> BreakUnless(IBooleanExpression condition, Block block)
        => condition.ToStatement(null, new Block { new Goto(GotoType.Break, block) });

    public static IEnumerable<IStatement> For(
        IEnumerable<IStatement>? initializer,
        IBooleanExpression? condition,
        IEnumerable<IStatement>? iterator,
        Block block)
    {
        if (condition != null)
            block.AddToFront(BreakUnless(condition, block));
        if (iterator != null)
            block.Add(iterator);
        BlockStatement blockStmt = new(block);
        return initializer != null ? initializer.Append(blockStmt) : blockStmt;
    }

    public static IEnumerable<IStatement> While(IBooleanExpression condition, Block block)
    {
        block.AddToFront(BreakUnless(condition, block));
        return new BlockStatement(block);
    }
}
