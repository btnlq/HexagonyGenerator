namespace HexagonyGenerator.Hexagony;

using System.Collections;

class Commands : IEnumerable<int>
{
	private readonly List<int> _cmds = new();

    public int this[int index]
    {
        get => _cmds[index];
        set => _cmds[index] = value;
    }

    public int Count => _cmds.Count;

    public IEnumerator<int> GetEnumerator() => _cmds.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _cmds.GetEnumerator();

    public void Add(int cmd) => _cmds.Add(cmd);

    public void AddRange(IEnumerable<int> cmds) => _cmds.AddRange(cmds);

    public void Add(params int[] cmds) => _cmds.AddRange(cmds);

    public void Reverse() => _cmds.Reverse();

    public static readonly Commands Empty = new();
}
