namespace HexagonyGenerator.Hexagony;

using Bytecode;

class Procedure
{
    private readonly Commands Main;
    private readonly Commands TrueBranch;
    private readonly Commands FalseBranch;
    private readonly ConditionType? Type;

    public Procedure(Bytecode.Procedure procedure)
    {
        Main = new Commands();
        var memory = new Memory(Main);
        foreach (var action in procedure.Actions)
            action.ApplyTo(memory);

        Commands SetIp(Bytecode.Procedure procedure, Commands? cmds = null)
        {
            cmds ??= new();
            if (procedure != Bytecode.Procedure.Exit)
                new Memory(cmds, memory).SetIp(procedure.Index - 1);
            else
                cmds.Add(Command.Exit);
            return cmds;
        }

        switch (procedure.Continuation)
        {
            case ConditionalContinuation continuation:
                memory.MoveTo(continuation.ConditionVar);
                TrueBranch = SetIp(continuation.TrueBranch);
                FalseBranch = SetIp(continuation.FalseBranch);
                Type = continuation.Type;
                break;
            case Continuation continuation:
                SetIp(continuation.Next, Main);
                TrueBranch = Commands.Empty;
                FalseBranch = Commands.Empty;
                break;
            default:
                throw new UnexpectedDefaultException();
        }
    }

    private static bool WriteSnake(HexagonColumnsEnumerator columns, CommandsEnumerator commands, bool continued = true)
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

    private bool WriteUnconditional(HexagonColumnsEnumerator columns)
    {
        int columnHeight = columns.Height(0);
        if (columnHeight < 3)
            return false;

        var main = new CommandsEnumerator(Main);
        var footer = Footers.UnconditionalShort[columns.NextWrap];
        int minPosition = Math.Max(0, main.Count - footer.Top(0) >> 1);
        int maxPosition = footer.MaxPosition(columns.Height(0), columns.Height(1), columns.Height(2));

        if (minPosition <= maxPosition)
        {
            footer.Write(columns, minPosition);
            columns.Write(0, 0, Dir.Down, main, minPosition + footer.Top(0));
            columns.Write(1, minPosition + footer.Top(1), Dir.Up, main, 1);
            columns.Write(2, minPosition + footer.Top(2) - 1, Dir.Up, main, main.Count);
            columns.Shift(3);
            return true;
        }
        else
        {
            return WriteSnake(columns, main, false);
        }
    }

    /*
    . _ a         aaa - entry
     _ b a        bbb - true branch
      $ b a       ccc - false branch
       c b a
        c b a
    */
    private void WriteBranches(HexagonColumnsEnumerator columns, out int falseBranchSize, out int trueBranchSize)
    {
        int wrapShift = columns.NextWrap == 1 ? 1 : 0;
        columns[0, 1 - wrapShift] = '_';
        columns[0, 2 - wrapShift] = '$';
        columns.Write(0, 3 - wrapShift, Dir.Down, FalseBranch);
        columns[1, 0] = '_';
        columns.Write(1, 1, Dir.Down, TrueBranch);
        falseBranchSize = FalseBranch.Count + 3 - wrapShift;
        trueBranchSize = TrueBranch.Count + 1;
    }

    private bool WriteDownward(HexagonColumnsEnumerator columns)
    {
        var footer = Footers.Downward(Type!.Value)[columns.NextWrap];

        WriteBranches(columns, out int falseBranchSize, out int trueBranchSize);
        int minPosition = footer.MinPosition(falseBranchSize, trueBranchSize, Main.Count > 0 ? 1 : 0);
        int maxPosition = footer.MaxPosition(columns.Height(0), columns.Height(1), columns.Height(2));

        if (minPosition > maxPosition)
            return false;

        var position = Main.Count - footer.Top(2);
        bool fit = position <= maxPosition;
        position = Math.Clamp(position, minPosition, maxPosition);
        footer.Write(columns, position);

        if (fit)
        {
            columns.Write(2, Main.Count - 1, Dir.Up, Main);
            columns.Shift(3);
            return true;
        }
        else
        {
            var main = new CommandsEnumerator(Main);
            var last = position + footer.Top(2) - 1;
            columns.Write(2, last, Dir.Up, main, last);
            columns[2, 0] = '/';
            columns.Shift(3);
            return WriteSnake(columns, main);
        }
    }

    private bool WriteUpward(HexagonColumnsEnumerator columns)
    {
        int nextWrap = columns.NextWrap;
        var footer = Footers.Upward(Type!.Value)[nextWrap];

        int wrapShift = nextWrap == 3 ? 1 : 0;

        WriteBranches(columns, out int falseBranchSize, out int trueBranchSize);
        int minPosition = footer.MinPosition(falseBranchSize, trueBranchSize, 0);
        int maxPosition = footer.MaxPosition(columns.Height(0), columns.Height(1), columns.Height(2) - 1 - wrapShift);

        if (minPosition > maxPosition)
            return false;

        footer.Write(columns, minPosition);

        var main = new CommandsEnumerator(Main);
        int mainStart = minPosition + footer.Bottom(2);
        int height = Math.Clamp(main.Count + mainStart + 4 - wrapShift >> 1, mainStart + 1, columns.Height(2) - wrapShift);

        columns.Write(2, mainStart, Dir.Down, main, height - mainStart - 1);
        columns[2, height - 1] = '_';
        height += wrapShift;
        columns[3, height - 2] = '|';

        if (main.Count <= height - 2)
        {
            columns.Write(3, height - 3, Dir.Up, main, main.Count);
            columns.Shift(4);
            return true;
        }
        else
        {
            columns.Write(3, height - 3, Dir.Up, main, height - 3);
            columns[3, 0] = '/';
            columns.Shift(4);
            return WriteSnake(columns, main);
        }
    }

    private bool WriteConditional(HexagonColumnsEnumerator columns)
    {
        const int bad = int.MaxValue;

        var start = columns.Column;
        var downwardEnumerator = columns.ReadonlyCopy;
        int downwardColumnsUsed = WriteDownward(downwardEnumerator) ? downwardEnumerator.Column - start : bad;
        var upwardEnumerator = columns.ReadonlyCopy;
        int upwardColumnsUsed = downwardColumnsUsed > 3 && WriteUpward(upwardEnumerator) ? upwardEnumerator.Column - start : bad;

        if (downwardColumnsUsed == bad && upwardColumnsUsed == bad)
            return false;

        if (columns.Readonly)
            columns.Shift(Math.Min(downwardColumnsUsed, upwardColumnsUsed));
        else
            if (downwardColumnsUsed < upwardColumnsUsed)
                WriteDownward(columns);
            else
                WriteUpward(columns);

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

    public bool Write(HexagonColumnsEnumerator columns)
    {
        if (columns.Available < 3)
            return false;

        columns[0, -2] = '>';
        columns[0, -1] = '<';
        if (!columns.IsFirst)
            columns[0, -3] = '_';
        columns[1, -2] = ')';
        columns[1, -1] = '$';

        bool result = Type == null ? WriteUnconditional(columns) : WriteConditional(columns);
        if (result)
            columns[-1, -2] = '<';
        return result;
    }
}
