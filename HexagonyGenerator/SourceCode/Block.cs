namespace HexagonyGenerator.SourceCode;

// If a block has non-null IsLoop:
// - It can be continued.
// - It has an implicit `continue` at the end.
class IsLoop
{
    public readonly IEnumerable<IStatement>? OnContinue; // Action to be executed before `continue`.

    public IsLoop(IEnumerable<IStatement>? onContinue = null)
    {
        OnContinue = onContinue;
    }
}

class Block : IEnumerable<IStatement>
{
    private readonly List<IStatement> _statements = new();
    public readonly IsLoop? IsLoop;

    public Block(IsLoop? isLoop = null)
    {
        IsLoop = isLoop;
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
