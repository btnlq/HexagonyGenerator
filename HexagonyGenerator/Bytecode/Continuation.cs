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

// ConditionVar is Type ? goto TrueBranch : goto FalseBranch
class ConditionalContinuation : IContinuation
{
    public readonly Variable ConditionVar;
    public readonly ConditionType Type;
    public readonly Procedure TrueBranch;
    public readonly Procedure FalseBranch;

    public ConditionalContinuation(Variable conditionVar, ConditionType type, Procedure trueBranch, Procedure falseBranch)
    {
        ConditionVar = conditionVar;
        Type = type;
        TrueBranch = trueBranch;
        FalseBranch = falseBranch;
    }
}
