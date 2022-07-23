namespace HexagonyGenerator.Hexagony.Writer;

/*
        1st row is main                      2nd row is main

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

class Shape
{
    private readonly int _size;
    private readonly bool _firstIsMain;

    public Shape(int size, bool firstIsMain)
    {
        _size = size;
        _firstIsMain = firstIsMain;
    }

    public int Size => _size;

    public int ColumnCount => _firstIsMain ? 2 * _size + 1 : 3 * _size;
    public int TopRow(int column) => column < _size + 2 ? 3 : _size + 3;
    public int RealColumn(int column) => column < _size + 2 ? column - 1 : column - 2 * _size - 2;
    public int ColumnHeight(int column)
    {
        if (column < _size + 2) // 1st row
        {
            column -= 1;
            return _firstIsMain || column >= _size - 1 ? 2 * _size - 2 - Math.Max(column, 0) : _size - 3;
        }
        else // 2nd row
        {
            column -= 2 * _size + 2;
            return _size - 2 - Math.Max(column, 0);
        }
    }

    public bool IsFirst(int column) => column == 0 || column == _size + 2;
    public int NextWrap(int column) => Math.Max(_size + 2 - column, 0);
}
