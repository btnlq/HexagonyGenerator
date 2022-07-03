namespace HexagonyGenerator.Hexagony;

class CommandsEnumerator
{
    private readonly Commands _cmds;
    private int _count;

    public int Count => _count;

    public CommandsEnumerator(Commands cmds)
    {
        _cmds = cmds;
        _count = _cmds.Count;
    }

    public IEnumerable<int> Get(int count)
    {
        int last = Math.Max(0, _count - count);
        while (_count > last)
        {
            _count--;
            yield return _cmds[_count];
        }
    }

    public void Skip(int count)
    {
        _count = Math.Max(0, _count - count);
    }
}
