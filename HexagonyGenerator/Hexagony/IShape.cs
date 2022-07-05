namespace HexagonyGenerator.Hexagony;

/*
 Big 1st shape (procedures 1,2,3)     Small 1st shape (procedures 1,2,3)
 Small 2nd shape (procedures 4,5)     Big 2nd shape (procedures 4,5,6,7)

        ) \ _ . . _ . .                      ) \ _ . . _ . .
       > ~ < > ) < > ) <                    > ~ < > ) < > ) <
      \ < $ . < $ . < $ .                  \ < $ . < $ . < $ .
     . . 1 1 1 2 2 2 3 3 3                . . 1 1 1 2 2 2 3 3 3
    . . . 1 1 1 2 2 2 3 3 3              . . . 1 1 1 2 2 2 3 3 3
   . . . . 1 1 1 2 2 2 3 3 3            . . . . 1 1 1 2 2 2 3 3 3
  . . . . . 1 1 1 2 2 2 3 3 3          . . . . . 1 1 1 2 2 2 3 3 3
 . . . _ . . 1 1 1 2 2 2 3 3 3        . . . _ . . _ . . _ . . . . .
  > ) < > ) < 1 1 1 2 2 2 3 3          > ) < > ) < > ) < > ) < . .
   < $ . < $ . 1 1 1 2 2 2 3            < $ . < $ . < $ . < $ . .
    4 4 4 5 5 5 1 1 1 2 2 2              4 4 4 5 5 5 6 6 6 7 7 7
     4 4 4 5 5 5 1 1 1 2 2                4 4 4 5 5 5 6 6 6 7 7
      4 4 4 5 5 5 1 1 1 2                  4 4 4 5 5 5 6 6 6 7
       4 4 4 5 5 5 1 1 1                    4 4 4 5 5 5 6 6 6
        4 4 4 5 5 5 1 1                      4 4 4 5 5 5 6 6
*/

interface IShape
{
    int TopRow { get; }
    int LeftColumn { get; }
    int RightColumn { get; }
    int ColumnHeight(int column);

    int Width => RightColumn - LeftColumn + 1;
}

class FirstShape : IShape
{
    private readonly int _size;
    private readonly bool _big;

    public FirstShape(int size, bool big) { _size = size; _big = big; }

    public int TopRow => 3;
    public int LeftColumn => -1;
    public int RightColumn => _size;
    public int ColumnHeight(int column) => _big ? 2 * _size - 2 - Math.Max(column, 0) : _size - 3;
}

class SecondShape : IShape
{
    private readonly int _size;
    private readonly bool _big;

    public SecondShape(int size, bool big) { _size = size; _big = big; }

    public int TopRow => _size + 3;
    public int LeftColumn => -_size;
    public int RightColumn => _big ? _size - 3 : -2;
    public int ColumnHeight(int column) => _size - 2 - Math.Max(column, 0);
}
