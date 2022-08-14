namespace HexagonyGenerator.SourceCode;

using Bytecode;

class SimpleActionList
{
    private readonly List<ISimpleAction> _statements = new();

    public IEnumerable<IStatement> Statements => _statements;

    public void AddAssignment(Assignment assignment)
        => _statements.Add(assignment.AsStatement());
    public void AddConditionalAssignment(ConditionalAssignment assignment)
        => _statements.Add(assignment.AsStatement());

    public void AddWriting(VariableType type, ISymbol symbol)
    {
        // write("4") -> write(4)
        // write(45,"6") -> write(456)
        if (type == VariableType.Byte && symbol is Integer integer
            && integer.Value <= '9' && integer.Value >= '0')
        {
            int digit = (int)integer.Value - 48;
            if (_statements.Count > 0 && _statements[^1] is SimpleAction<Writing> writingAction)
            {
                var writing = writingAction.Action;
                if (writing.Type == VariableType.Int && writing.Symbol is Integer prevInteger)
                {
                    var value = prevInteger.Value;
                    if (!value.IsZero && Value.Abs(value) < 0x10FFFF / 10)
                    {
                        value = value * 10 + (value.Sign >= 0 ? digit : -digit);
                        _statements[^1] = new Writing(VariableType.Int, new Integer(value)).AsStatement();
                        return;
                    }
                }
            }
            _statements.Add(new Writing(VariableType.Int, new Integer(digit)).AsStatement());
            return;
        }
        _statements.Add(new Writing(type, symbol).AsStatement());
    }
}
