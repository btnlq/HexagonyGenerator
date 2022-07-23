namespace HexagonyGenerator.Hexagony.Writer;

enum Dir { Down = 1, Up = -1 }

class HexagonColumnsEnumerator
{
    private readonly Hexagon? _hxg;
    private readonly Shape _shape;

    public int Column { get; private set; }

    public HexagonColumnsEnumerator(Shape shape, Hexagon? hxg = null)
    {
        _shape = shape;
        _hxg = hxg;
    }

    public bool IsFirst => _shape.IsFirst(Column);

    public int NextWrap => _shape.NextWrap(Column);

    public bool Readonly => _hxg == null;

    public HexagonColumnsEnumerator ReadonlyCopy => new(_shape) { Column = Column };

    public int Available => _shape.ColumnCount - Column;

    public void Shift(int columns) => Column += columns;

    public int Height(int column) => _shape.ColumnHeight(Column + column);

    public int this[int column, int row]
    {
        set
        {
            if (_hxg != null)
            {
                column += Column;
                _hxg[_shape.TopRow(column) + row, _shape.RealColumn(column)] = value;
            }
        }
    }

    public void Write(int column, int row, Dir dir, CommandsEnumerator commands, int count = int.MaxValue)
    {
        if (_hxg == null)
            commands.Skip(count);
        else
        {
            column += Column;
            row += _shape.TopRow(column);
            column = _shape.RealColumn(column);
            foreach (var cmd in commands.Take(count))
            {
                _hxg[row, column] = cmd;
                row += (int)dir;
            }
        }
    }
}
