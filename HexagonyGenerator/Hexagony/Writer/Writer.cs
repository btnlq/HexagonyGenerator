﻿namespace HexagonyGenerator.Hexagony.Writer;

class Writer
{
    private readonly List<Procedure> _procedures;
    private readonly int _startIndex;

    public Writer(List<Procedure> procedures, int startIndex)
    {
        _procedures = procedures;
        _startIndex = startIndex;
    }

    private bool WriteSnake(CommandsEnumerator commands, bool continued = true)
    {
        while (columns.Available >= 2)
        {
            int columnHeight = columns.Height(0);
            if (columnHeight < 3)
                return false;

            int wrapShift = columns.NextWrap == 1 ? 1 : 0;

            bool fit = continued && commands.Count <= 2 * columnHeight - 4 - wrapShift;
            int height = fit ? Math.Max(commands.Count + 5 + wrapShift >> 1, 3) : columnHeight - wrapShift;
            if (continued) columns[0, 0] = '<';
            int continuedShift = continued ? 1 : 0;
            columns.Write(0, continuedShift, Dir.Down, commands, height - 1 - continuedShift);
            columns[0, height - 1] = '_';
            height += wrapShift;
            columns[1, height - 2] = '|';
            columns.Write(1, height - 3, Dir.Up, commands, fit ? height - 2 : height - 3);
            if (!fit)
                columns[1, 0] = '/';
            columns.Shift(2);
            if (fit)
                return true;
            continued = true;
        }

        return false;
    }

    private bool WriteUnconditional(Procedure procedure)
    {
        int columnHeight = columns.Height(0);
        if (columnHeight < 3)
            return false;

        var main = procedure.Main.Reversed();
        var footer = Footers.UnconditionalShort[columns.NextWrap];
        int minPosition = Math.Max(0, main.Count - footer.Top(0) >> 1);
        int maxPosition = footer.MaxPosition(columns.Height(0), columns.Height(1), columns.Height(2));

        if (minPosition <= maxPosition)
        {
            footer.Write(columns, minPosition);
            columns.Write(0, 0, Dir.Down, main, minPosition + footer.Top(0));
            columns.Write(1, minPosition + footer.Top(1), Dir.Up, main, 1);
            columns.Write(2, minPosition + footer.Top(2) - 1, Dir.Up, main);
            columns.Shift(3);
            return true;
        }
        else
        {
            return WriteSnake(main, false);
        }
    }

    /*
    . _ a         aaa - entry
     _ b a        bbb - true branch
      $ b a       ccc - false branch
       c b a
        c b a
    */
    private void WriteBranches(Procedure procedure, out int falseBranchSize, out int trueBranchSize)
    {
        int wrapShift = columns.NextWrap == 1 ? 1 : 0;
        columns[0, 1 - wrapShift] = '_';
        columns[0, 2 - wrapShift] = '$';
        columns.Write(0, 3 - wrapShift, Dir.Down, procedure.FalseBranch.Reversed());
        columns[1, 0] = '_';
        columns.Write(1, 1, Dir.Down, procedure.TrueBranch.Reversed());
        falseBranchSize = procedure.FalseBranch.Count + 3 - wrapShift;
        trueBranchSize = procedure.TrueBranch.Count + 1;
    }

    private bool WriteDownward(Procedure procedure)
    {
        var footer = Footers.Downward(procedure.Type!.Value)[columns.NextWrap];

        WriteBranches(procedure, out int falseBranchSize, out int trueBranchSize);
        var length = procedure.Main.Count;
        int minPosition = footer.MinPosition(falseBranchSize, trueBranchSize, length > 0 ? 1 : 0);
        int maxPosition = footer.MaxPosition(columns.Height(0), columns.Height(1), columns.Height(2));

        if (minPosition > maxPosition)
            return false;

        var position = length - footer.Top(2);
        bool fit = position <= maxPosition;
        position = Math.Clamp(position, minPosition, maxPosition);
        footer.Write(columns, position);

        var main = procedure.Main.Reversed();
        if (fit)
        {
            columns.Write(2, length - 1, Dir.Up, main);
            columns.Shift(3);
            return true;
        }
        else
        {
            var last = position + footer.Top(2) - 1;
            columns.Write(2, last, Dir.Up, main, last);
            columns[2, 0] = '/';
            columns.Shift(3);
            return WriteSnake(main);
        }
    }

    private bool WriteUpward(Procedure procedure)
    {
        int nextWrap = columns.NextWrap;
        var footer = Footers.Upward(procedure.Type!.Value)[nextWrap];

        int wrapShift = nextWrap == 3 ? 1 : 0;

        WriteBranches(procedure, out int falseBranchSize, out int trueBranchSize);
        int minPosition = footer.MinPosition(falseBranchSize, trueBranchSize, 0);
        int maxPosition = footer.MaxPosition(columns.Height(0), columns.Height(1), columns.Height(2) - 1 - wrapShift);

        if (minPosition > maxPosition)
            return false;

        footer.Write(columns, minPosition);

        var main = procedure.Main.Reversed();
        int mainStart = minPosition + footer.Bottom(2);
        int height = Math.Clamp(main.Count + mainStart + 4 - wrapShift >> 1, mainStart + 1, columns.Height(2) - wrapShift);

        columns.Write(2, mainStart, Dir.Down, main, height - mainStart - 1);
        columns[2, height - 1] = '_';
        height += wrapShift;
        columns[3, height - 2] = '|';

        if (main.Count <= height - 2)
        {
            columns.Write(3, height - 3, Dir.Up, main);
            columns.Shift(4);
            return true;
        }
        else
        {
            columns.Write(3, height - 3, Dir.Up, main, height - 3);
            columns[3, 0] = '/';
            columns.Shift(4);
            return WriteSnake(main);
        }
    }

    private bool WriteConditional(Procedure procedure)
    {
        const int bad = int.MaxValue;

        var initColumns = columns;
        columns = initColumns.ReadonlyCopy;
        int downwardColumnsUsed = WriteDownward(procedure) ? columns.Column - initColumns.Column : bad;
        columns = initColumns.ReadonlyCopy;
        int upwardColumnsUsed = downwardColumnsUsed > 3 && WriteUpward(procedure) ? columns.Column - initColumns.Column : bad;
        columns = initColumns;

        if (downwardColumnsUsed == bad && upwardColumnsUsed == bad)
            return false;

        if (columns.Readonly)
            columns.Shift(Math.Min(downwardColumnsUsed, upwardColumnsUsed));
        else
            if (downwardColumnsUsed < upwardColumnsUsed)
            WriteDownward(procedure);
        else
            WriteUpward(procedure);

        return true;
    }

    /*
           ) \ _ . . _ . . . .
          > ~ < > ) < > ) . . <
         \ < $ . < $ . < $ . . .
        . . 1 1 1 2 2 2 3 3 3 3 3
       . . . 1 1 1 2 2 2 3 3 3 3 3
      . . . . 1 1 1 2 2 2 3 3 3 3 3
    */

    private bool Write(Procedure procedure)
    {
        if (columns.Available < 3)
            return false;

        columns[0, -2] = '>';
        columns[0, -1] = '<';
        if (!columns.IsFirst)
            columns[0, -3] = '_';
        columns[1, -2] = ')';
        columns[1, -1] = '$';

        bool result = procedure.Type == null ? WriteUnconditional(procedure) : WriteConditional(procedure);
        if (result)
            columns[-1, -2] = '<';
        return result;
    }

    private void WriteInitializer(Hexagon hxg)
    {
        hxg[1, 0] = '~';
        hxg[2, -2] = '\\';

        if (_startIndex < 3)
        {
            hxg[0, 0] = ").("[_startIndex];
            hxg[0, 1] = '\\';
        }
        else
        {
            int y = 0;
            foreach (char c in (_startIndex - 2).ToString(System.Globalization.CultureInfo.InvariantCulture))
            {
                hxg[0, y] = c;
                y++;
                if (hxg[0, y] == '_') y++;
            }
            while (hxg[1, y - 1] != ')') y++;
            hxg[0, y] = '\\';
        }
    }

    private HexagonColumnsEnumerator columns = null!;

    public Hexagon Write(Shape shape)
    {
        var hxg = new Hexagon(shape.Size);
        columns = new(shape, hxg);
        _ = _procedures.All(Write);
        WriteInitializer(hxg);
        return hxg;
    }

    public bool CanWrite(Shape shape)
    {
        columns = new(shape);
        return _procedures.All(Write);
    }
}
