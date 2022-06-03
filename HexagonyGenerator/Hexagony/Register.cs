namespace HexagonyGenerator.Hexagony;

class Register
{
	private readonly int _index;

	private Register(Bytecode.Variable variable)
	{
		_index = 2 * variable.Location;
		if (_index < 0) _index -= 2;
	}

	private Register()
    {
		_index = -2;
    }

	public static Register Ip { get; } = new Register();

	public static implicit operator Edge(Register reg) => new(reg._index);
	public static explicit operator Register(Bytecode.Variable variable) => new(variable);

	private bool IsDown => (_index & 3) == 2;

	public Edge Left => new(IsDown ? _index - 1 : _index + 1);
	public Edge Right => new(IsDown ? _index + 1 : _index - 1);

	// < 0 - left
	// = 0 - both
	// > 0 - right
	public int ClosestNeighbourTo(Edge edge) => (IsDown ? 1 : -1) * System.Math.Sign(edge.Index - _index);
}
