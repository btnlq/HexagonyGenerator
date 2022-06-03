namespace HexagonyGenerator.SourceCode;

// IStatement = Loop | Conditional | Goto | Exit | ISimpleAction
interface IStatement : IEnumerable<IStatement>
{
    IEnumerator<IStatement> IEnumerable<IStatement>.GetEnumerator()
        => new SelfEnumerator<IStatement>(this);
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => new SelfEnumerator<IStatement>(this);
}

class SelfEnumerator<T> : IEnumerator<T>
{
    private readonly T _item;
    private bool _hasNext;

    public SelfEnumerator(T item)
    {
        _item = item;
        _hasNext = true;
    }

    public bool MoveNext()
    {
        bool hasNext = _hasNext;
        _hasNext = false;
        return hasNext;
    }

    public T Current => _item;

    object? System.Collections.IEnumerator.Current => Current;

    void System.Collections.IEnumerator.Reset() => _hasNext = false;

    public void Dispose() { }
}