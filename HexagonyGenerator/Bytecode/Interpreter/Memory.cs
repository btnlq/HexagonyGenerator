namespace HexagonyGenerator.Bytecode.Interpreter;

class Memory : IMemory
{
    private readonly Dictionary<int, Value> _data = new();
    private readonly Hexagony.Interpreter.Reader _reader;
    private readonly Hexagony.Interpreter.Writer _writer = new();

    public Memory(Hexagony.Interpreter.Reader reader)
    {
        _reader = reader;
    }

    public string GetOutput() => _writer.GetOutput();

    public Value this[Variable variable]
    {
        get { return _data.TryGetValue(variable.Location, out Value value) ? value : 0; }
        private set { _data[variable.Location] = value; }
    }

    private Value Get(ISymbol symbol) => symbol switch
    {
        Variable from => this[from],
        Integer value => value.Value,
        Reading reading => reading.Type switch
        {
            VariableType.Byte => _reader.ReadByte(),
            VariableType.Int => _reader.ReadInt(),
            _ => throw new UnexpectedDefaultException(),
        },
        _ => throw new UnexpectedDefaultException(),
    };

    // IMemory

    public void Set(Variable dest, ISymbol value)
    {
        this[dest] = Get(value);
    }

    public void Set(Variable dest, ISymbol left, BinOp op, ISymbol right)
    {
        this[dest] = op.Compute(Get(left), Get(right));
    }

    public void WriteByte(ISymbol symbol)
    {
        var value = Get(symbol);
        _writer.WriteByte(value);
    }

    public void WriteInt(ISymbol symbol)
    {
        var value = Get(symbol);
        _writer.WriteInt(value);
    }
}
