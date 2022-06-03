namespace HexagonyGenerator.Hexagony.Interpreter;

readonly record struct Pos(int X, int Y)
{
    public static readonly Pos Zero = new();
}
