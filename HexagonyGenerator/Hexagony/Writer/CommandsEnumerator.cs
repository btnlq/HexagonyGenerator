namespace HexagonyGenerator.Hexagony.Writer;

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

    public IEnumerable<int> Take(int count)
    {
        int iter = _count;
        _count = Math.Max(0, _count - count);
        while (iter > _count)
        {
            iter--;
            yield return _cmds[iter];
        }
    }

    public void Skip(int count)
    {
        _count = Math.Max(0, _count - count);
    }
}

static class CommandsEx
{
    public static CommandsEnumerator Reversed(this Commands cmds) => new(cmds);
}
