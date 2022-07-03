namespace HexagonyGenerator.Hexagony;

class Footer
{
    private readonly string[] _columns;
    private readonly int[] _shifts;

    public int Top(int i) => _shifts[i];
    public int Bottom(int i) => _columns[i].Length;

    public Footer(params string[] columns)
    {
        System.Diagnostics.Debug.Assert(columns.Length == 3);

        _columns = columns;
        _shifts = new int[3];
        for (int i = 0; i < 3; i++)
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
        System.Diagnostics.Debug.Assert(columnHeights.Length == 3);

        int pos = int.MaxValue;
        for (int i = 0; i < 3; i++)
            pos = Math.Min(pos, columnHeights[i] - _columns[i].Length);
        return pos;
    }

    public int MinPosition(params int[] columnCmds)
    {
        System.Diagnostics.Debug.Assert(columnCmds.Length == 3);

        int pos = 0;
        for (int i = 0; i < 3; i++)
            pos = Math.Max(pos, columnCmds[i] - _shifts[i]);
        return pos;
    }

    public void Write(HexagonColumnsEnumerator columns, int position)
    {
        if (!columns.Readonly)
        {
            for (int i = 0; i < 3; i++)
            {
                var column = _columns[i];
                for (int j = _shifts[i]; j < column.Length; j++)
                    columns[i, position + j] = column[j];
            }
        }
    }
}
