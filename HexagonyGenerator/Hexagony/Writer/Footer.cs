namespace HexagonyGenerator.Hexagony.Writer;

class Footer
{
    private readonly string[] _columns;
    private readonly int[] _shifts;

    public int Top(int i) => _shifts[i];
    public int Bottom(int i) => _columns[i].Length;

    public Footer(params string[] columns)
    {
        _columns = columns;
        _shifts = new int[columns.Length];
        for (int i = 0; i < columns.Length; i++)
        {
            var column = _columns[i];
            int shift = 0;
            while (shift < column.Length && column[shift] == ' ')
                shift++;
            _shifts[i] = shift;
        }
    }

    public int MaxPosition(params int[] columnHeights)
    {
        System.Diagnostics.Debug.Assert(columnHeights.Length == _columns.Length);

        int pos = int.MaxValue;
        for (int i = 0; i < _columns.Length; i++)
            pos = Math.Min(pos, columnHeights[i] - _columns[i].Length);
        return pos;
    }

    public int MinPosition(params int[] columnCmds)
    {
        System.Diagnostics.Debug.Assert(columnCmds.Length == _columns.Length);

        int pos = 0;
        for (int i = 0; i < _columns.Length; i++)
            pos = Math.Max(pos, columnCmds[i] - _shifts[i]);
        return pos;
    }

    public void Write(HexagonColumnsEnumerator columns, int position)
    {
        if (!columns.Readonly)
        {
            for (int i = 0; i < _columns.Length; i++)
            {
                var column = _columns[i];
                for (int j = _shifts[i]; j < column.Length; j++)
                    columns[i, position + j] = column[j];
            }
        }
    }
}

class FooterGroup
{
    private readonly Footer[] _footers;

    public FooterGroup(params Footer[] footers)
    {
        _footers = footers;
    }

    public Footer this[int nextWrap] => _footers[nextWrap < _footers.Length ? nextWrap : 0];
}

static class Footers
{
    /*

    aaa - entry
    bbb - true branch
    ccc - false branch

    -- DOWNWARD CONDITIONAL --      --- UPWARD CONDITIONAL ---      - UNCODNITIONAL -
    x > 0     x < 0     x != 0      x > 0     x < 0     x != 0      short     long

    c b a     c b a     c b a       c b .     c b .     c b .       b . a     b / < a
     c b a     c b a     c b a       c b .     c b .     c b .       b . a     b a a a
      c b a     c ~ ~     c b a       c b .     c ~ .     c b .       b . a     b a a a
       c > /     ~ > /     c b $       c > <     ~ > <     c > <       a . |     a a a a
        _ . .     _ . .     c > <       _ . a     _ . ~     _ $ |       a a .     a | a |
         . . .     . . .     _ $ .       . . a     . . a     . ~ .       _ . .     _ . _ .
                              . ~ ~       . . a     . . a     | > <
                               . > /                           _ . ~
                                > . .                           . . a
                                 . . .                           . . a
    */

    public static FooterGroup Snake = new(
        new(" _", "|"),
        new("_.", "|")
    );

    public static FooterGroup UnconditionalShort = new(
        new("  _", " .", "|"),
        new(" _.", " .", "|"),
        new(" _", "..", "|")
    );

    private static FooterGroup Generate(string column0, string column1, string column2, string? column2shift2 = null) => new(
        new(column0, column1, column2),
        new(column0[1..] + ".", column1, column2),
        new(column0, column1 + ".", column2shift2 ?? column2)
    );

    private static readonly FooterGroup[] _downwardFooters =
    {
        Generate(" _", ">", "/"),
        Generate(" ~_", "~>", "~/"),
        Generate("  _..>", " >$~>", "$<.~/"),
    };

    private static readonly FooterGroup[] _upwardFooters =
    {
        Generate(" _", ">", "<"),
        Generate(" ~_", "~>", " <~"),
        Generate(" _.|_", ">$~>", "<|.<~", "<.|<~"),
    };

    public static FooterGroup Downward(Bytecode.ConditionType type) => _downwardFooters[(int)type];
    public static FooterGroup Upward(Bytecode.ConditionType type) => _upwardFooters[(int)type];
}
