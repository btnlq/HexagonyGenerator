namespace HexagonyGenerator.Hexagony.Compiler;

using Bytecode;

class Memory : IMemory
{
    private readonly Grid _grid;

    public Memory(Commands cmds, Memory memory)
    {
        _grid = new(cmds, memory._grid);
    }

    public Memory(Commands cmds)
    {
        _grid = new(cmds, Register.Ip);
    }

    public void SetIp(int value)
    {
        _grid.Set(Register.Ip, value);
        _grid.TurnToBus();
    }

    public void MoveTo(Variable dest) => _grid.MoveTo((Register)dest);

    private void Set(Edge dest, Register register) => _grid.Set(dest, register);
    private void Set(Edge dest, Value value, bool put = false) => _grid.Set(dest, value, put);
    private void CallOp(Edge dest, char op, bool mutable = true) => _grid.CallOp(dest, op, mutable);
    private void CallOp(Register dest, BinOp op) => _grid.CallBinOp(dest, op.ToCommand());

    public void Set(Variable dest, ISymbol symbol) => Set((Register)dest, symbol);

    private void Read(Edge dest, VariableType type)
        => CallOp(dest, type == VariableType.Int ? Command.ReadInt : Command.ReadByte);

    private void Set(Edge dest, ISymbol symbol, bool put = false)
    {
        switch (symbol)
        {
            case Variable variable:
                Set(dest, (Register)variable);
                break;
            case Integer integer:
                Set(dest, integer.Value, put);
                break;
            case Reading reading:
                Read(dest, reading.Type);
                break;
            default:
                throw new UnexpectedDefaultException();
        }
    }

    private static bool IsSmallInt(Integer integer, out int value)
    {
        var bigValue = integer.Value;
        bool isSmall = Value.Abs(bigValue) <= 10;
        value = isSmall ? (int)bigValue : 0;
        return isSmall;
    }

    private bool TryOptimize(Register dest, ISymbol leftSymbol, BinOp op, ISymbol rightSymbol)
    {
        if (leftSymbol is Integer leftInteger)
        {
            if (rightSymbol is Integer rightInteger)
            {
                var value = Bytecode.BinOpEx.Compute(op, leftInteger.Value, rightInteger.Value);
                Set(dest, value);
                return true;
            }
            else
            {
                if (IsSmallInt(leftInteger, out int leftValue))
                {
                    switch ((leftValue, op))
                    {
                        case (0, BinOp.Add): // x = 0 + y
                        case (1, BinOp.Mul): // x = 1 * y
                            Set(dest, rightSymbol); // to: x = y
                            return true;
                        case (0, BinOp.Sub): // x = 0 - y
                        case (-1, BinOp.Mul): // x = -1 * y
                            Set(dest, rightSymbol); // to: x = -y
                            CallOp(dest, Command.Negate);
                            return true;
                        case (0, BinOp.Mul): // x = 0 * y
                            if (rightSymbol is Reading reading)
                                Read(dest, reading.Type);
                            Set(dest, Value.Zero); // to: x = 0
                            return true;
                        case (1, BinOp.Add): // x = 1 + y
                            Set(dest, rightSymbol); // to: x = y; x++
                            CallOp(dest, Command.Increment);
                            return true;
                        case (-1, BinOp.Add): // x = -1 + y
                            Set(dest, rightSymbol); // to: x = y; x--
                            CallOp(dest, Command.Decrement);
                            return true;
                        case (10, BinOp.Mul): // x = 10 * y
                            Set(dest, rightSymbol); // to: x = y; x.add('0')
                            CallOp(dest, Command.Mul10);
                            return true;
                    }
                }
            }
        }
        else
        {
            if (rightSymbol is Integer rightInteger)
            {
                if (IsSmallInt(rightInteger, out int rightValue))
                {
                    switch ((op, rightValue))
                    {
                        case (BinOp.Add, 0): // x = y + 0
                        case (BinOp.Sub, 0): // x = y - 0
                        case (BinOp.Mul, 1): // x = y * 1
                        case (BinOp.Div, 1): // x = y / 1
                            Set(dest, leftSymbol); // to: x = y
                            return true;
                        case (BinOp.Mul, 0): // x = y * 0
                        case (BinOp.Mod, 1): // x = y % 1
                        case (BinOp.Mod, -1): // x = y % -1
                            Set(dest, Value.Zero); // to: x = 0
                            return true;
                        case (BinOp.Div, 0): // x = y / 0
                        case (BinOp.Mod, 0): // x = y % 0
                            throw new CompilationException("Division by zero");
                        case (BinOp.Add, 1):  // x = y + 1
                        case (BinOp.Sub, -1): // y = y - -1
                            Set(dest, leftSymbol); // to: x = y; x++
                            CallOp(dest, Command.Increment);
                            return true;
                        case (BinOp.Add, -1): // x = y + -1
                        case (BinOp.Sub, 1):  // x = y - 1
                            Set(dest, leftSymbol); // to: x = y; x--
                            CallOp(dest, Command.Decrement);
                            return true;
                        case (BinOp.Mul, -1): // x = y * -1
                        case (BinOp.Div, -1): // x = y / -1
                            Set(dest, leftSymbol); // to: x = -y
                            CallOp(dest, Command.Negate);
                            return true;
                        case (BinOp.Mul, 10): // x = y * 10
                            Set(dest, leftSymbol); // to: x = y; x += '0'
                            CallOp(dest, '0');
                            return true;
                    }
                }
            }
        }

        return false;
    }

    public void Set(Variable variableDest, ISymbol left, BinOp op, ISymbol right)
    {
        var dest = (Register)variableDest;

        if (TryOptimize(dest, left, op, right))
            return;

        if (left is Variable leftVar)
        {
            if (right is Variable rightVar)
                Set(dest, (Register)leftVar, op, (Register)rightVar);
            else
                Set(dest, (Register)leftVar, op, right);
        }
        else
        {
            if (right is Variable rightVar)
                Set(dest, left, op, (Register)rightVar);
            else
                Set(dest, left, op, right);
        }
    }

    private void Set(Register dest, Register left, BinOp op, Register right)
    {
        if (op.Swappable() && dest.ClosestNeighbourTo(left) > dest.ClosestNeighbourTo(right))
        {
            (right, left) = (left, right);
        }

        if (dest.ClosestNeighbourTo(left) > 0)
        {
            if (dest.ClosestNeighbourTo(right) < 0)
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

        CallOp(dest, op);
    }

    private void Set(Register dest, Register left, BinOp op, ISymbol rightSymbol)
    {
        if (op.Swappable() && dest.ClosestNeighbourTo(left) > 0)
        {
            Set(dest.Right, left);
            Set(dest.Left, rightSymbol);
        }
        else
        {
            Set(dest.Left, left);
            Set(dest.Right, rightSymbol);
        }

        CallOp(dest, op);
    }

    private void Set(Register dest, ISymbol leftSymbol, BinOp op, Register right)
    {
        if (op.Swappable() && dest.ClosestNeighbourTo(right) < 0)
        {
            Set(dest.Left, right);
            Set(dest.Right, leftSymbol);
        }
        else
        {
            Set(dest.Right, right);
            Set(dest.Left, leftSymbol);
        }

        CallOp(dest, op);
    }

    private void Set(Register dest, ISymbol leftSymbol, BinOp op, ISymbol rightSymbol)
    {
        Set(dest.Left, leftSymbol);
        Set(dest.Right, rightSymbol);
        CallOp(dest, op);
    }

    public void Write(ISymbol symbol, VariableType type)
    {
        Edge temp;

        if (symbol is Variable variable)
            temp = (Register)variable;
        else
        {
            temp = _grid.TempEdge;
            Set(temp, symbol, type == VariableType.Byte);
        }

        CallOp(temp, type == VariableType.Int ? Command.WriteInt : Command.WriteByte, false);
    }

    public void Read(VariableType type) => Read(_grid.TempEdge, type);
}

static class BinOpEx
{
    public static bool Swappable(this BinOp op) => op == BinOp.Add || op == BinOp.Mul;
    public static char ToCommand(this BinOp op) => (char)op;
}
