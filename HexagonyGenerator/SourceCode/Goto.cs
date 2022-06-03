namespace HexagonyGenerator.SourceCode;

enum GotoType { Break, Continue }

class Goto : IStatement
{
    public readonly GotoType Type;
    public readonly Block Block;

    public Goto(GotoType type, Block block)
    {
        Type = type;
        Block = block;
    }
}

class Exit : IStatement { }
