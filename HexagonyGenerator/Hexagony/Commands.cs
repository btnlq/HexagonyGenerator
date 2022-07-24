namespace HexagonyGenerator.Hexagony;

using System.Collections;

class Commands : IEnumerable<int>
{
    private readonly List<int> _cmds;

    public Commands() { _cmds = new(); }

    public int this[int index] => _cmds[index];

    public int Count => _cmds.Count;

    public IEnumerator<int> GetEnumerator() => _cmds.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _cmds.GetEnumerator();

    public void Add(int cmd) => _cmds.Add(cmd);

    public void Add(params int[] cmds) => _cmds.AddRange(cmds);

    public static readonly Commands Empty = new();

    public void Pop(int count) => _cmds.RemoveRange(_cmds.Count - count, count);
}
