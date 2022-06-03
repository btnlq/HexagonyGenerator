namespace HexagonyGenerator.SourceCode;

class Block : IEnumerable<IStatement>
{
    private readonly List<IStatement> _statements = new();
    public readonly IEnumerable<IStatement>? OnContinue; // action to be executed on `continue this;`

    public Block(IEnumerable<IStatement>? onContinue = null)
    {
        OnContinue = onContinue;
    }
    
    public void Add(IStatement statement)
    {
        _statements.Add(statement);
    }

    public void Add(IEnumerable<IStatement> list)
    {
        _statements.AddRange(list);
    }

    public void AddToFront(IEnumerable<IStatement> list)
    {
        _statements.InsertRange(0, list);
    }

    public IEnumerator<IStatement> GetEnumerator() => _statements.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _statements.GetEnumerator();
}
