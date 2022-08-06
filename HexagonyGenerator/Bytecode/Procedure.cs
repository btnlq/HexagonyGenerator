namespace HexagonyGenerator.Bytecode;

class Procedure
{
    public int Index;
    public readonly List<IAction> Actions = new();
    public IContinuation? Continuation;

    public static readonly Procedure Exit = new();
    static Procedure() { Exit.Continuation = new Continuation(Exit); }
}
