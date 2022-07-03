namespace HexagonyGenerator.Hexagony;

using System.Text;
using static Math;

class Hexagon
{
    private readonly List<Commands> _right = new();
    private readonly List<Commands> _left = new();

    private int _size;
    public int Size => _size;

    private static void Put(List<Commands> rows, int x, int y, int cmd)
    {
        while (x >= rows.Count)
            rows.Add(new());

        var row = rows[x];

        while (y >= row.Count)
            row.Add(Command.Nop);
        row[y] = cmd;
    }

    public int this[int x, int y]
    {
        get
        {
            var rows = y < 0 ? _left : _right;
            if (x >= rows.Count)
                return Command.Nop;
            if (y < 0)
                y = ~y;
            var row = rows[x];
            if (y >= row.Count)
                return Command.Nop;
            return row[y];
        }
        set
        {
            if (x < 0)
                throw new System.ArgumentOutOfRangeException(null, $"x >= 0, but x = {x}");
            if (x + y < 0)
                throw new System.ArgumentOutOfRangeException(null, $"x+y >= 0, but x = {x}, y = {y}");

            if (y >= 0)
                Put(_right, x, y, value);
            else
                Put(_left, x, ~y, value);

            if (value != Command.Nop)
            {
                _size = Max(_size, Abs(y));         // -n <= y <= n
                _size = Max(_size, x + y + 1 >> 1); // 0 <= x+y <= 2n
                _size = Max(_size, x + 1 >> 1);     // 0 <= x <= 2n
            }
        }
    }

    public string ToText(bool pretty)
    {
        var sb = new StringBuilder();

        for (int x = 0; x <= 2 * _size; x++)
        {
            if (pretty)
                sb.Append(' ', Abs(_size - x));

            int leftY = -Min(x, _size);
            int rightY = Min(_size, 2 * _size - x);
            for (int y = leftY; y <= rightY; y++)
            {
                int cmd = this[x, y];
                if (cmd <= char.MaxValue)
                    sb.Append((char)cmd);
                else
                    sb.Append(char.ConvertFromUtf32(cmd));
                if (pretty)
                    sb.Append(' ');
            }

            if (pretty)
                sb[^1] = '\n';
        }

        if (pretty)
            sb.Length--; // remove last '\n'
        else
        {
            int toRemove = 6 * _size - 1;
            while (toRemove > 0 && sb[^1] == Command.Nop)
            {
                sb.Length--;
                toRemove--;
            }
        }

        return sb.ToString();
    }

    public void Stats(out int bytes, out int chars, out int operators)
    {
        bytes = chars = 3 * Size * (Size + 1) + 1;
        operators = 0;

        foreach (var rows in new[] { _left, _right })
            foreach (var row in rows)
                foreach (var cmd in row)
                    if (cmd != Command.Nop)
                    {
                        operators++;
                        bytes += new Rune(cmd).Utf8SequenceLength;
                    }

        bytes -= operators;
    }
}
