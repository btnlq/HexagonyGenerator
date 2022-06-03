namespace HexagonyGenerator.Hexagony.Interpreter;

/*
   2--1
  /    \
 3      0
  \    /
   4--5
*/

readonly struct Dir
{
    private readonly int _x;

    private Dir(int x)
    {
        _x = x % 6;
        if (_x < 0)
            _x += 6;
    }

    private static readonly Pos[] Units =
    {
        new(0, 1),
        new(-1, 1),
        new(-1, 0),
        new(0, -1),
        new(1, -1),
        new(1, 0),
    };

    public static Pos operator +(Pos a, Dir b)
    {
        var unit = Units[b._x];
        return new(a.X + unit.X, a.Y + unit.Y);
    }

    public static Pos operator -(Pos a, Dir b)
    {
        var unit = Units[b._x];
        return new(a.X - unit.X, a.Y - unit.Y);
    }

    public static implicit operator int(Dir dir) => dir._x;
    public static implicit operator Dir(int x) => new(x);

    public static readonly Dir Right = new(0);
}
