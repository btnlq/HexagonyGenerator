namespace HexagonyGenerator.Bytecode;

interface IContinuation
{
}

// goto Next
class Continuation : IContinuation
{
    public readonly Procedure Next;

    public Continuation(Procedure next)
    {
        Next = next;
    }
}

enum ConditionType { Positive, Negative, Nonzero }

// ConditionSymbol is Type ? goto TrueBranch : goto FalseBranch
class ConditionalContinuation : IContinuation
{
    public readonly ModifiableSymbol ConditionSymbol;
    public readonly ConditionType Type;
    public readonly Procedure TrueBranch;
    public readonly Procedure FalseBranch;

    public ConditionalContinuation(ModifiableSymbol conditionSymbol, ConditionType type, Procedure trueBranch, Procedure falseBranch)
    {
        ConditionSymbol = conditionSymbol;
        Type = type;
        TrueBranch = trueBranch;
        FalseBranch = falseBranch;
    }
}
