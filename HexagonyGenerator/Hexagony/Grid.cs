namespace HexagonyGenerator.Hexagony;

class Grid
{
    /*
     *           |       |
     *           4       8      <- top registers
     * ...       |       |
     *    \     / \     / \
     *     1   3   5   7   ...  <- bus
     *      \ /     \ /
     *       |       |
     *       2       6          <- bottom registers
     *       |       |
     */

    private enum MemDir { Up, Down }

    private int _mpIndex;
    private MemDir _mpDir;

    private readonly Commands _cmds;

    public Grid(Commands cmds, Register register)
    {
        _mpIndex = ((Edge)register).Index;
        _mpDir = (_mpIndex & 3) == 2 ? MemDir.Up : MemDir.Down;
        _cmds = cmds;
    }

    public Grid(Commands cmds, Grid grid)
    {
        _mpIndex = grid._mpIndex;
        _mpDir = grid._mpDir;
        _cmds = cmds;
    }

    public void MoveTo(Edge edge, bool withCopy = false)
    {
        bool toRight = edge.Index > _mpIndex;
        int dir = toRight ? 1 : -1;

        while (_mpIndex != edge.Index)
        {
            MemDir mpDir;
            bool turnRight;

            bool busStep = false;

            if ((_mpIndex & 1) == 0)
            {
                bool isTop = (_mpIndex & 3) == 0;
                mpDir = isTop ? MemDir.Down : MemDir.Up;
                turnRight = toRight ^ isTop;
            }
            else if (_mpIndex + dir == edge.Index)
            {
                turnRight = (_mpIndex & 3) == 1;
                mpDir = turnRight ^ toRight ? MemDir.Up : MemDir.Down;
            }
            else
            {
                turnRight = (_mpIndex & 3) == 3;
                mpDir = turnRight ^ toRight ? MemDir.Down : MemDir.Up;
                busStep = true;
            }

            bool forward = _mpDir == mpDir;
            _cmds.Add(forward ?
                turnRight ? Command.TurnRight : Command.TurnLeft :
                turnRight ? Command.TurnLeftBackwards : Command.TurnRightBackwards); // TODO: remove '}, }', "{, {"

            _mpIndex += busStep ? 2 * dir : dir;
            if (busStep)
                _mpDir = 1 - _mpDir;

            if (withCopy)
            {
                if (forward)
                    ReverseMp();

                _cmds.Add('a');
                if (turnRight)
                    _cmds.Add(Command.Negate);
                _cmds.Add(Command.Copy);
            }
        }
    }

    private void ReverseMp()
    {
        _cmds.Add(Command.ReverseMp);
        _mpDir = 1 - _mpDir;
    }

    public void CallOp(Edge dest, char op)
    {
        MoveTo(dest);
        _cmds.Add(op);
    }

    public void CallBinOp(Register dest, char op)
    {
        MoveTo(dest);
        TurnToBus();
        _cmds.Add(op);
    }

    private void Put(Value value)
    {
        bool isNegative = value.Sign < 0;
        if (isNegative) value = -value;

        if (value <= 0x10FFFF)
        {
            int number = (int)value;

            if (number == 0 || number == 96)
                _cmds.Add(number + 1, Command.Decrement);
            else if (number == 9 || number == 91)
                _cmds.Add(number - 1, Command.Increment);
            else if (10 <= number && number <= 13 || 32 <= number && number <= 64 || 0xD800 <= number && number < 0xDFFF)
                _cmds.Add(number / 10, 48 + number % 10);
            else if (92 <= number && number <= 95)
                _cmds.Add(8, Command.Increment, 48 + number % 10);
            else
                _cmds.Add(number);
        }
        else
        {
            List<int> digits = new();
            while (value > 0x10FFFF)
            {
                value = Value.DivRem(value, 10, out var digit);
                digits.Add((int)digit + 48);
            }
            _cmds.Add((int)value);
            digits.Reverse();
            _cmds.AddRange(digits);
        }

        if (isNegative)
            _cmds.Add(Command.Negate);
    }

    public void Set(Edge dest, Value value)
    {
        MoveTo(dest);
        Put(value);
    }

    // $toIndex = $fromIndex
    public void Set(Edge dest, Edge from)
    {
        if (dest.Index == from.Index)
            return;
        MoveTo(from);
        MoveTo(dest, true);
    }

    public void TurnToBus()
    {
        var rightDir = (_mpIndex & 3) == 2 ? MemDir.Up : MemDir.Down;
        if (_mpDir != rightDir)
            ReverseMp();
    }

    public Edge TempEdge => new(_mpIndex | 1);
}
