namespace HexagonyGenerator.Hexagony;

class Procedure
{
    public readonly Commands Main;
    public readonly Commands TrueBranch;
    public readonly Commands FalseBranch;
    public readonly Bytecode.ConditionType? Type;

    public Procedure(Commands main)
    {
        Main = main;
        TrueBranch = Commands.Empty;
        FalseBranch = Commands.Empty;
        Type = null;
    }

    public Procedure(Commands main, Commands trueBranch, Commands falseBranch, Bytecode.ConditionType type)
    {
        Main = main;
        TrueBranch = trueBranch;
        FalseBranch = falseBranch;
        Type = type;
    }
}
