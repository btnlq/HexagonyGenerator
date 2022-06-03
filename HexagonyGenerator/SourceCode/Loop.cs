namespace HexagonyGenerator.SourceCode;

class Loop : IStatement
{
    public readonly Block Block;

    public Loop(Block block)
    {
        Block = block;
    }
}
