namespace HexagonyGenerator.Bytecode;

class Procedure
{
    public readonly List<IAction> Actions = new();
    public IContinuation? Continuation;

    public static readonly Procedure Exit = new();
}
