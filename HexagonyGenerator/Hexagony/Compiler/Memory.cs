namespace HexagonyGenerator.Hexagony.Compiler;

using Bytecode;

class Memory : IMemory
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

    private void Set(Edge dest, Register register) => _grid.Set(dest, register);
    private void Set(Edge dest, Value value, bool put = false) => _grid.Set(dest, value, put);
    private void CallOp(Edge dest, char op, bool mutable = true) => _grid.CallOp(dest, op, mutable);
    private void CallOp(Register dest, BinOp op) => _grid.CallBinOp(dest, op.ToCommand());

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

    public void Set(Variable variableDest, ISymbol left, BinOp op, ISymbol right)
    {
        var dest = new Register(variableDest);

        bool swap = op == BinOp.Add || op == BinOp.Sub || op == BinOp.Mul;

        swap =
            left is VariableSymbol leftVar ?
                right is VariableSymbol rightVar ?
                    Set(dest, leftVar, swap, rightVar) :
                    Set(dest, leftVar, swap, right) :
                right is VariableSymbol rightVar2 ?
                    Set(dest, left, swap, rightVar2) :
                    Set(dest, left, right);

        CallOp(dest, op);
        if (swap && op == BinOp.Sub)
            CallOp(dest, Command.Negate);
    }

    private bool Set(Register dest, VariableSymbol left, bool swap, VariableSymbol right)
    {
        swap = swap && dest.ClosestNeighbourTo(left.Variable) > dest.ClosestNeighbourTo(right.Variable);

        if (swap)
            (right, left) = (left, right);

        if (dest.ClosestNeighbourTo(left.Variable) > 0)
        {
            if (dest.ClosestNeighbourTo(right.Variable) < 0)
            {
                Set(dest, left);
                Set(dest.Right, right);
                Set(dest.Left, dest);
            }
            else
            {
                Set(dest.Left, left);
                Set(dest.Right, right);
            }
        }
        else
        {
            Set(dest.Right, right);
            Set(dest.Left, left);
        }

        return swap;
    }

    private bool Set(Register dest, VariableSymbol left, bool swap, ISymbol rightSymbol)
    {
        swap = swap && dest.ClosestNeighbourTo(left.Variable) > 0;

        if (swap)
        {
            Set(dest.Right, left);
            Set(dest.Left, rightSymbol);
        }
        else
        {
            Set(dest.Left, left);
            Set(dest.Right, rightSymbol);
        }

        return swap;
    }

    private bool Set(Register dest, ISymbol leftSymbol, bool swap, VariableSymbol right)
    {
        swap = swap && dest.ClosestNeighbourTo(right.Variable) < 0;

        if (swap)
        {
            Set(dest.Left, right);
            Set(dest.Right, leftSymbol);
        }
        else
        {
            Set(dest.Right, right);
            Set(dest.Left, leftSymbol);
        }

        return swap;
    }

    private bool Set(Register dest, ISymbol leftSymbol, ISymbol rightSymbol)
    {
        Set(dest.Left, leftSymbol);
        Set(dest.Right, rightSymbol);
        return false;
    }

    public void Write(ISymbol symbol, VariableType type)
    {
        var edge = SetAny(symbol, type == VariableType.Byte);
        CallOp(edge, type == VariableType.Int ? Command.WriteInt : Command.WriteByte, false);
    }

    public void Read(VariableType type) => Read(_grid.TempEdge, type);
}

static class BinOpEx
{
    public static char ToCommand(this BinOp op) => (char)op;
}
