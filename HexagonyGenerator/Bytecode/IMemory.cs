namespace HexagonyGenerator.Bytecode;

interface IMemory
{
    // $dest = value
    void Set(Variable dest, ISymbol value);

    // $dest = $left `op` $right
    void Set(Variable dest, ISymbol left, BinOp op, ISymbol right);

    // $dest = $conditionSymbol > 0 ? $trueValue : $falseValue
    void Set(Variable dest, ModifiableSymbol conditionSymbol, ISymbol trueValue, ISymbol falseValue);

    void Write(ISymbol symbol, VariableType type);

    void Read(VariableType type);
}
