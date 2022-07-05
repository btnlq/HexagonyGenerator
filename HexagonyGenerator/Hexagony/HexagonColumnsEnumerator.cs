namespace HexagonyGenerator.Hexagony;

enum Dir { Down = 1, Up = -1 }

class HexagonColumnsEnumerator
{
    private readonly Hexagon? _hxg;
    private readonly IShape _shape;

    public int Column { get; private set; }

    public HexagonColumnsEnumerator(IShape shape, Hexagon? hxg = null)
    {
        _shape = shape;
        _hxg = hxg;
        Column = _shape.LeftColumn;
    }

    public bool IsFirst => Column == _shape.LeftColumn;

    public bool Readonly => _hxg == null;

    public HexagonColumnsEnumerator ReadonlyCopy => new(_shape) { Column = Column };

    public int Available => _shape.RightColumn + 1 - Column;

    public void Shift(int columns) => Column += columns;

    public int Height(int column) => _shape.ColumnHeight(Column + column);

    public int this[int column, int row]
    {
        set
        {
            if (_hxg != null)
                _hxg[_shape.TopRow + row, Column + column] = value;
        }
    }

    public void Write(int column, int row, Dir dir, CommandsEnumerator commands, int count)
    {
        if (_hxg == null)
            commands.Skip(count);
        else
        {
            column += Column;
            row += _shape.TopRow;
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
            row += _shape.TopRow;
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                _hxg[row, column] = commands[i];
                row += (int)dir;
            }
        }
    }
}
