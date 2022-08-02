namespace HexagonyGenerator.Bytecode;

// ISymbol = Integer | VariableSymbol | ReadingSymbol 
interface ISymbol { }

class VariableSymbol : ModifiableSymbol
{
    public readonly Variable Variable;

    public VariableSymbol(Variable variable)
    {
        Variable = variable;
    }
}

class ReadingSymbol : ModifiableSymbol
{
    public readonly VariableType Type;

    public ReadingSymbol(VariableType type)
    {
        Type = type;
    }
}

enum ModifierType
{
    Negate,
    Increment,
    Decrement,
    Mul10,
}

class ModifiableSymbol : ISymbol
{
    private List<ModifierType>? _modifiers;

    private static readonly ModifierType[] _inverse =
        { ModifierType.Negate, ModifierType.Decrement, ModifierType.Increment, (ModifierType)(-1) };

    public void Modify(ModifierType modifier)
    {
        _modifiers ??= new();
        if (_modifiers.Count > 0 && _modifiers[^1] == _inverse[(int)modifier])
            _modifiers.RemoveAt(_modifiers.Count - 1);
        else
            _modifiers.Add(modifier);
    }

    public int ModifiersCount => _modifiers == null ? 0 : _modifiers.Count;

    public IEnumerable<ModifierType> Modifiers => _modifiers ?? Enumerable.Empty<ModifierType>();
    public IEnumerable<ModifierType> ModifiersReverse
    {
        get
        {
            if (_modifiers != null)
                for (int i = _modifiers.Count - 1; i >= 0; i--)
                    yield return _modifiers[i];
        }
    }
}
