namespace HexagonyGenerator.Hexagony.Compiler;

using Bytecode;

partial class Memory : IMemory
{
    private readonly Grid _grid;

    public Memory(Commands cmds, Memory memory)
    {
        _grid = new(cmds, memory._grid);
    }

    public Memory(Commands cmds, bool noInput)
    {
        _grid = new(cmds, Register.Ip, noInput);
    }

    public void SetIp(int value)
    {
        _grid.Set(Register.Ip, value);
        _grid.TurnToBus();
    }

    public Edge SetAny(ISymbol dest, bool put = false)
    {
        Edge edge;

        switch (dest)
        {
            case VariableSymbol symbol:
                if (symbol.ModifiersCount == 0)
                {
                    edge = new Register(symbol.Variable);
                    _grid.MoveTo(edge);
                }
                else
                {
                    edge = new Register(symbol.Variable).Temp;
                    Set(edge, symbol);
                }
                break;
            default:
                edge = _grid.TempEdge;
                Set(edge, dest, put);
                break;
        }

        return edge;
    }

    private void Set(Edge dest, Edge from) => _grid.Set(dest, from);
    private void Set(Edge dest, Value value, bool put = false) => _grid.Set(dest, value, put);
    private void CallOp(Edge dest, char op, bool mutable = true) => _grid.CallOp(dest, op, mutable);

    public void Set(Variable dest, ISymbol symbol) => Set(new Register(dest), symbol);

    private void Read(Edge dest, VariableType type)
        => CallOp(dest, type == VariableType.Int ? Command.ReadInt : Command.ReadByte);

    private void Set(Edge dest, VariableSymbol variable)
    {
        Set(dest, new Register(variable.Variable));
        Modify(dest, variable);
    }

    private void Set(Edge dest, ISymbol symbol, bool put = false)
    {
        switch (symbol)
        {
            case VariableSymbol variable:
                Set(dest, variable);
                break;
            case Integer integer:
                Set(dest, integer.Value, put);
                break;
            case ReadingSymbol reading:
                Read(dest, reading.Type);
                Modify(dest, reading);
                break;
            default:
                throw new UnexpectedDefaultException();
        }
    }

    private static readonly char[] _modifierCommands =
        { Command.Negate, Command.Increment, Command.Decrement, Command.Mul10 };

    private void Modify(Edge dest, ModifiableSymbol symbol)
    {
        foreach (var modifier in symbol.Modifiers)
            CallOp(dest, _modifierCommands[(int)modifier]);
    }

    public void Write(ISymbol symbol, VariableType type)
    {
        var edge = SetAny(symbol, type == VariableType.Byte);
        CallOp(edge, type == VariableType.Int ? Command.WriteInt : Command.WriteByte, false);
    }

    public void Read(VariableType type) => Read(_grid.TempEdge, type);
}
