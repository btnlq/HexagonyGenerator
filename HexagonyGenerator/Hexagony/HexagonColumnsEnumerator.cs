namespace HexagonyGenerator.Hexagony;

/*
       1st shape is main                    2nd shape is main

        ) \ _ . . _ . .                      ) \ _ . . _ . .
       > ~ < > ) < > ) <                    > ~ < > ) < > ) <
      \ < $ . < $ . < $ .                  \ < $ . < $ . < $ .
     . . 1 1 1 2 2 2 3 3 3                . . 1 1 1 2 2 2 3 3 3
    . . . 1 1 1 2 2 2 3 3 3              . . . 1 1 1 2 2 2 3 3 3
   . . . . 1 1 1 2 2 2 3 3 3            . . . . 1 1 1 2 2 2 3 3 3
  . . . . . 1 1 1 2 2 2 3 3 3          . . . . . 1 1 1 2 2 2 3 3 3
 . . . _ . . 1 1 1 2 2 2 3 3 3        . . . _ . . _ . . _ . . 3 3 3
  > ) < > ) < 1 1 1 2 2 2 3 3          > ) < > ) < > ) < > ) < 3 3
   < $ . < $ . 1 1 1 2 2 2 3            < $ . < $ . < $ . < $ . 3
    4 4 4 5 5 5 1 1 1 2 2 2              4 4 4 5 5 5 6 6 6 7 7 7
     4 4 4 5 5 5 1 1 1 2 2                4 4 4 5 5 5 6 6 6 7 7
      4 4 4 5 5 5 1 1 1 2                  4 4 4 5 5 5 6 6 6 7
       4 4 4 5 5 5 1 1 1                    4 4 4 5 5 5 6 6 6
        4 4 4 5 5 5 1 1                      4 4 4 5 5 5 6 6
*/

enum Dir { Down = 1, Up = -1 }

class HexagonColumnsEnumerator
{
    private readonly int _size;
    private readonly bool _firstIsMain;
    private readonly Hexagon? _hxg;

    public int Column { get; private set; }

    public HexagonColumnsEnumerator(int size, bool firstIsMain, Hexagon? hxg = null)
    {
        _size = size;
        _firstIsMain = firstIsMain;
        _hxg = hxg;
    }

    private int ColumnCount => _firstIsMain ? 2 * _size + 1 : 3 * _size;
    private int TopRow(int column) => column < _size + 2 ? 3 : _size + 3;
    private int RealColumn(int column) => column < _size + 2 ? column - 1 : column - 2 * _size - 2;
    private int ColumnHeight(int column)
    {
        if (column < _size + 2)
        {
            column -= 1;
            return _firstIsMain || column >= _size - 1 ? 2 * _size - 2 - Math.Max(column, 0) : _size - 3;
        }
        else
        {
            column -= 2 * _size + 2;
            return _size - 2 - Math.Max(column, 0);
        }
    }

    public bool IsFirst => Column == 0 || Column == _size + 2;

    public int NextWrap => Math.Max(_size + 2 - Column, 0);

    public bool Readonly => _hxg == null;

    public HexagonColumnsEnumerator ReadonlyCopy => new(_size, _firstIsMain) { Column = Column };

    public int Available => ColumnCount - Column;

    public void Shift(int columns) => Column += columns;

    public int Height(int column) => ColumnHeight(Column + column);

    public int this[int column, int row]
    {
        set
        {
            if (_hxg != null)
            {
                column += Column;
                _hxg[TopRow(column) + row, RealColumn(column)] = value;
            }
        }
    }

    public void Write(int column, int row, Dir dir, CommandsEnumerator commands, int count)
    {
        if (_hxg == null)
            commands.Skip(count);
        else
        {
            column += Column;
            row += TopRow(column);
            column = RealColumn(column);
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
            row += TopRow(column);
            column = RealColumn(column);
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                _hxg[row, column] = commands[i];
                row += (int)dir;
            }
        }
    }
}
