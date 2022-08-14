namespace HexagonyGenerator.Hexagony.Compiler;

struct Register
{
    private readonly int _index;

    public Register(Bytecode.Variable variable)
    {
        _index = 2 * variable.Location;
        if (_index < 0) _index -= 2;
    }

    private Register(int index)
    {
        _index = index;
    }

    public static Register Ip { get; } = new Register(-2);

    public static implicit operator Edge(Register reg) => new(reg._index);

    private bool IsDown => (_index & 3) == 2;

    public Edge Left => new(IsDown ? _index - 1 : _index + 1);
    public Edge Right => new(IsDown ? _index + 1 : _index - 1);
    public Edge Neighbour(int sign) => new(IsDown ? _index + sign : _index - sign);
    public Edge Temp => new(_index - 1);

    // -1 - left
    //  0 - both
    //  1 - right
    public int ClosestNeighbourTo(Bytecode.Variable variable)
        => (IsDown ? 1 : -1) * Math.Sign(new Register(variable)._index - _index);
}
