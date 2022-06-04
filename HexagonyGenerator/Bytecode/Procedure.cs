namespace HexagonyGenerator.Bytecode;

class Procedure
{
    public int Index = NotIndexed;
    public readonly List<IAction> Actions = new();
    public IContinuation? Continuation;

    public const int NotIndexed = int.MinValue;
    public static readonly Procedure Exit = new();
    static Procedure() { Exit.Continuation = new Continuation(Exit); }
}
