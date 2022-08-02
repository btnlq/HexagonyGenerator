namespace HexagonyGenerator.SourceCode;

using Bytecode;

static class Modifier
{
    private static ModifiableSymbol Put(ModifiableSymbol symbol, ModifierType type)
    {
        symbol.Modify(type);
        return symbol;
    }

    private static ISymbol? Zero(ModifiableSymbol symbol) => symbol is ReadingSymbol ? null : new Integer(0);
    public static ModifiableSymbol Negate(ModifiableSymbol symbol) => Put(symbol, ModifierType.Negate);
    public static ModifiableSymbol Increment(ModifiableSymbol symbol) => Put(symbol, ModifierType.Increment);
    public static ModifiableSymbol Decrement(ModifiableSymbol symbol) => Put(symbol, ModifierType.Decrement);
    public static ModifiableSymbol Mul10(ModifiableSymbol symbol) => Put(symbol, ModifierType.Mul10);

    public static ModifiableSymbol? Add(ModifiableSymbol symbol, int value) => value switch
    {
        0 => symbol,
        1 => Increment(symbol),
        -1 => Decrement(symbol),
        _ => null,
    };

    public static ISymbol? Mul(ModifiableSymbol symbol, int value) => value switch
    {
        0 => Zero(symbol),
        1 => symbol,
        -1 => Negate(symbol),
        10 => Mul10(symbol),
        -10 => Mul10(Negate(symbol)),
        _ => null,
    };

    public static ModifiableSymbol? Div(ModifiableSymbol symbol, int value) => value switch
    {
        0 => throw new CompilationException("Division by zero"),
        1 => symbol,
        -1 => Negate(symbol),
        _ => null,
    };

    public static ISymbol? Mod(ModifiableSymbol symbol, int value) => value switch
    {
        0 => throw new CompilationException("Division by zero"),
        1 or -1 => Zero(symbol),
        _ => null,
    };
}
