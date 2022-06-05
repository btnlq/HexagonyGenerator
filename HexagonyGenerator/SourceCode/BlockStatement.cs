namespace HexagonyGenerator.SourceCode;

class BlockStatement : IStatement
{
    public readonly Block Block;

    public BlockStatement(Block block)
    {
        Block = block;
    }
}
