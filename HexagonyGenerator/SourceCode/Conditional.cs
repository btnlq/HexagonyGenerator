namespace HexagonyGenerator.SourceCode;

using Bytecode;

class Conditional : IStatement
{
    public readonly Variable Variable;
    public readonly ConditionType Type;
    public readonly Block? TrueBranch;
    public readonly Block? FalseBranch;

    public Conditional(Variable variable, ComparisonOp op, Block? trueBlock, Block? falseBlock)
    {
        Variable = variable;
        (Type, bool reversed) = op switch
        {
            ComparisonOp.Gt => (ConditionType.Positive, false),
            ComparisonOp.Le => (ConditionType.Positive, true),
            ComparisonOp.Lt => (ConditionType.Negative, false),
            ComparisonOp.Ge => (ConditionType.Negative, true),
            ComparisonOp.Ne => (ConditionType.Nonzero, false),
            ComparisonOp.Eq => (ConditionType.Nonzero, true),
            _ => throw new UnexpectedDefaultException(),
        };
        TrueBranch = reversed ? falseBlock : trueBlock;
        FalseBranch = reversed ? trueBlock : falseBlock;
    }
}
