namespace HexagonyGenerator.Bytecode;

class ConditionalAssignment : IAction
{
    public readonly Variable Dest;
    public readonly ModifiableSymbol ConditionSymbol;
    public readonly ISymbol TrueValue;
    public readonly ISymbol FalseValue;

    public ConditionalAssignment(Variable dest, ModifiableSymbol conditionSymbol, ISymbol trueValue, ISymbol falseValue)
    {
        Dest = dest;
        ConditionSymbol = conditionSymbol;
        TrueValue = trueValue;
        FalseValue = falseValue;
    }

    public void ApplyTo(IMemory memory)
    {
        memory.Set(Dest, ConditionSymbol, TrueValue, FalseValue);
    }
}
