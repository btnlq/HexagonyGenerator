namespace HexagonyGenerator.Hexagony;

using System.Text;

class Hexagon
{
    private readonly int _size;
    private readonly int[][] _grid;

    public int Size => _size;

    public Hexagon(int size)
    {
        _size = size;
        _grid = new int[_size * 2 + 1][];
        for (int i = 0; i <= 2 * size; i++)
        {
            var row = new int[Math.Min(size + i, 3 * size - i) + 1];
            System.Array.Fill(row, '.');
            _grid[i] = row;
        }
    }

    // -n <= y <= n
    // 0 <= x+y <= 2n
    // 0 <= x <= 2n
    private int GetIndex(int x, int y)
    {
        if (x > _size)
        {
            if (x <= 2 * _size && y >= -_size && x + y <= 2 * _size)
                return y + _size;
        }
        else
        {
            if (x >= 0 && y <= _size && x + y >= 0)
                return x + y;
        }

        throw new System.ArgumentOutOfRangeException(null, $"({x}, {y}) is outside the grid of size {_size}");
    }

    public int this[int x, int y]
    {
        get
        {
            int index = GetIndex(x, y);
            return _grid[x][index];
        }
        set
        {
            int index = GetIndex(x, y);
            _grid[x][index] = value;
        }
    }

    public string ToText(bool pretty)
    {
        var sb = new StringBuilder();

        for (int x = 0; x <= 2 * _size; x++)
        {
            if (pretty)
                sb.Append(' ', Math.Abs(_size - x));

            foreach (int cmd in _grid[x])
            {
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
        bytes = chars = 3 * _size * (_size + 1) + 1;
        operators = 0;

        foreach (var row in _grid)
            foreach (var cmd in row)
                if (cmd != Command.Nop)
                {
                    operators++;
                    bytes += new Rune(cmd).Utf8SequenceLength;
                }

        bytes -= operators;
    }
}
