namespace HexagonyGenerator.Hexagony.Compiler;

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

    private readonly bool _noInput;
    private readonly Commands _cmds;
    private readonly Dictionary<int, Value> _cache = new();

    public Grid(Commands cmds, Register register, bool noInput)
    {
        _mpIndex = ((Edge)register).Index;
        _mpDir = (_mpIndex & 3) == 2 ? MemDir.Up : MemDir.Down;
        _noInput = noInput;
        _cmds = cmds;
    }

    public Grid(Commands cmds, Grid grid)
    {
        _mpIndex = grid._mpIndex;
        _mpDir = grid._mpDir;
        _noInput = grid._noInput;
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

            bool reversed = _mpDir != mpDir;
            if (_cmds.Count > 0 && _cmds[^1] == TurnCommand(turnRight ^ reversed, !reversed))
                _cmds.Pop(1);
            else
                _cmds.Add(TurnCommand(turnRight ^ reversed, reversed));

            _mpIndex += busStep ? 2 * dir : dir;
            if (busStep)
                _mpDir = 1 - _mpDir;

            if (withCopy)
            {
                if (!reversed)
                    ReverseMp();

                if (_cache.TryGetValue(_mpIndex, out var value))
                {
                    if (value.Sign <= 0 ^ turnRight)
                        _cmds.Add(value.IsZero ? Command.Increment : Command.Negate);
                    _cache.Remove(_mpIndex);
                }
                else
                {
                    if (_noInput)
                        _cmds.Add(turnRight ? Command.ReadByte : 'a');
                    else
                    {
                        _cmds.Add('a');
                        if (turnRight)
                            _cmds.Add(Command.Negate);
                    }
                }

                _cmds.Add(Command.Copy);
            }
        }
    }

    private static char TurnCommand(bool turnRight, bool backwards) =>
        backwards ?
            turnRight ? Command.TurnRightBackwards : Command.TurnLeftBackwards :
            turnRight ? Command.TurnRight : Command.TurnLeft;

    private void ReverseMp()
    {
        char cmd = Command.ReverseMp;

        int count = _cmds.Count;
        if (count >= 2)
        {
            int x = _cmds[count - 2];
            int y = _cmds[count - 1];
            if (x == '\'' && y == '{') { _cmds.Pop(2); cmd = '"'; }
            if (x == '"' && y == '}') { _cmds.Pop(2); cmd = '\''; }
        }

        _cmds.Add(cmd);
        _mpDir = 1 - _mpDir;
    }

    public void CallOp(Edge dest, char op, bool mutable)
    {
        MoveTo(dest);
        _cmds.Add(op);
        if (mutable)
            _cache.Remove(_mpIndex);
    }

    public void CallBinOp(Register dest, char op)
    {
        MoveTo(dest);
        TurnToBus();
        _cmds.Add(op);
        _cache.Remove(_mpIndex);
    }

    private static readonly Stack<int> _digits = new();

    private void Put(Value value)
    {
        _cache[_mpIndex] = value;

        bool isNegative = value.Sign < 0;
        if (isNegative) value = -value;

        if (_noInput)
        {
            if (value.IsZero)
            {
                _cmds.Add(Command.ReadInt);
                return;
            }
            if (value.IsOne && isNegative)
            {
                _cmds.Add(Command.ReadByte);
                return;
            }
        }

        bool useBytes = Configuration.ValueEncoding == ValueEncoding.Bytes;

        int maxValue = useBytes ? 128 : 0x10FFFF;
        while (value > maxValue)
        {
            value = Value.DivRem(value, 10, out var digit);
            _digits.Push((int)digit + 48);
        }

        int number = (int)value;

        if (number == 0 || number == 96 || number == 126)
            _cmds.Add(number + 1, Command.Decrement);
        else if (number == 9 || number == 91 || number == 123 || (number == 128 && useBytes))
            _cmds.Add(number - 1, Command.Increment);
        else if (10 <= number && number <= 13 || 32 <= number && number <= 64 || 0xD800 <= number && number < 0xDFFF)
            _cmds.Add(number / 10, 48 + number % 10);
        else if (92 <= number && number <= 95)
            _cmds.Add(8, Command.Increment, 48 + number % 10);
        else if (124 <= number && number <= 125)
            _cmds.Add(1, '2', number - (124 - '4'));
        else
            _cmds.Add(number);

        while (_digits.Count > 0)
            _cmds.Add(_digits.Pop());

        if (isNegative)
            _cmds.Add(Command.Negate);
    }

    public void Set(Edge dest, Value value, bool put = false)
    {
        if (_cache.TryGetValue(dest.Index, out var oldValue))
            if (put ? (value & 255) == (oldValue & 255) : value == oldValue)
                return;

        if (put && Configuration.ValueEncoding == ValueEncoding.ForceChars && value.Sign >= 0 && value <= 126)
        {
            int x = (int)value;
            if (x == 0 || 9 <= x && x <= 13 || 32 <= x && x <= 64 || 91 <= x && x <= 96 || 123 <= x && x <= 126)
                value = 256 + x;
        }

        MoveTo(dest);
        Put(value);
    }

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
