namespace HexagonyGenerator.Hexagony;

enum Dir { Down = 1, Up = -1 }

class HexagonColumnsEnumerator
{
    private readonly Hexagon? _hxg;
    private readonly int _size;

    // Shape
    private int TopRow => 3;
    private int LeftColumn => -1;
    private int RightColumn => _size;
    private int ColumnHeight(int column) => 2 * _size - 2 - Math.Max(column, 0);
    // End Shape

    public int Column { get; private set; }

    public HexagonColumnsEnumerator(int size, Hexagon? hxg = null)
    {
        _size = size;
        _hxg = hxg;
        Column = LeftColumn;
    }

    public bool IsFirst => Column == LeftColumn;

    public bool Readonly => _hxg == null;

    public HexagonColumnsEnumerator ReadonlyCopy => new(_size) { Column = Column };

    public int Available => RightColumn + 1 - Column;

    public void Shift(int columns) => Column += columns;

    public int Height(int column) => ColumnHeight(Column + column);

    public int this[int column, int row]
    {
        set
        {
            if (_hxg != null)
                _hxg[TopRow + row, Column + column] = value;
        }
    }

    public void Write(int column, int row, Dir dir, CommandsEnumerator commands, int count)
    {
        if (_hxg == null)
            commands.Skip(count);
        else
        {
            column += Column;
            row += TopRow;
            foreach (var cmd in commands.Get(count))
            {
                _hxg[row, column] = cmd;
                row += (int)dir;
            }
        }
    }

    public void Write(int column, int row, Dir dir, Commands commands)
    {
        if (_hxg != null)
        {
            column += Column;
            row += TopRow;
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                _hxg[row, column] = commands[i];
                row += (int)dir;
            }
        }
    }
}
