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

    /*

    aaa - entry
    bbb - true branch
    ccc - false branch

    -- DOWNWARD CONDITIONAL --      --- UPWARD CONDITIONAL ---      - UNCODNITIONAL -
    x > 0     x < 0     x != 0      x > 0     x < 0     x != 0      short     long


    . _ a     . _ a     . _ a       . _ .     . _ .     . _ .       b . a     b / < a
     _ b a     _ b a     _ b a       _ b .     _ b .     _ b .       b . a     b a a a
      $ b a     $ b a     $ b a       $ b .     $ b .     $ b .       b . a     b a a a
       c b a     c b a     c b a       c b .     c b .     c b .       b . a     b a a a
        c b a     c ~ ~     c b a       c b .     c ~ .     c b .       a . a     a a a a
         c > /     ~ > /     c b $       c > <     ~ > <     c > <       a . |     a a a a
          _ . .     _ . .     c > <       _ . a     _ . ~     _ $ .       a a .     a | a |
           . . .     . . .     _ $ .       . . a     . . a     / ~ /       _ . .     _ . _ .
                                . ~ ~       . . a     . . a     . > <
                                 . > /                           _ . ~
                                  > . .                           . . a
                                   . . .                           . . a
    */

    private static readonly Footer[] _downwardFooters =
    {
        new(" _", ">", "/"),
        new(" ~_", "~>", "~/"),
        new("  _..>", " >$~>", "$<.~/"),
    };

    private static readonly Footer[] _upwardFooters =
    {
        new(" _", ">", "<"),
        new(" ~_", "~>", " <~"),
        new(" _/._", ">$~>", "<./<~"),
    };

    private Footer DownwardFooter => _downwardFooters[(int)Type!];
    private Footer UpwardFooter => _upwardFooters[(int)Type!];

    private static bool WriteSnake(HexagonColumnsEnumerator columns, CommandsEnumerator commands)
    {
        while (columns.Available >= 2)
        {
            int columnHeight = columns.Height(0);
            if (columnHeight < 3)
                return false;

            bool fit = commands.Count <= 2 * columnHeight - 4;
            int height = fit ? Math.Max(commands.Count + 5 >> 1, 3) : columnHeight;
            columns[0, 0] = '<';
            columns.Write(0, 1, Dir.Down, commands, height - 2);
            columns[0, height - 1] = '_';
            columns[1, height - 2] = '|';
            columns.Write(1, height - 3, Dir.Up, commands, fit ? height - 2 : height - 3);
            if (!fit)
                columns[1, 0] = '/';
            columns.Shift(2);
            if (fit)
                return true;
        }

        return false;
    }

    private bool WriteUnconditional(HexagonColumnsEnumerator columns)
    {
        int columnHeight = columns.Height(0);
        if (columnHeight < 3)
            return false;

        var main = new CommandsEnumerator(Main);

        int height = Math.Max((main.Count >> 1) + 2, 3);
        if (height <= columnHeight)
        {
            columns.Write(0, 0, Dir.Down, main, height - 1);
            columns[0, height - 1] = '_';
            columns.Write(1, height - 2, Dir.Up, main, 1);
            columns[2, height - 3] = '|';
            columns.Write(2, height - 4, Dir.Up, main, height - 3);
            columns.Shift(3);
            return true;
        }
        else
        {
            height = columnHeight;
            columns.Write(0, 0, Dir.Down, main, height - 1);
            columns[0, height - 1] = '_';
            columns[1, height - 2] = '|';
            columns.Write(1, height - 3, Dir.Up, main, height - 3);
            columns[1, 0] = '/';
            columns.Shift(2);
            return WriteSnake(columns, main);
        }
    }

    private bool WriteDownward(HexagonColumnsEnumerator columns)
    {
        var footer = DownwardFooter;

        int minPosition = footer.MinPosition(3 + FalseBranch.Count, 1 + TrueBranch.Count, Main.Count > 0 ? 1 : 0);
        int maxPosition = footer.MaxPosition(columns.Height(0), columns.Height(1), columns.Height(2));

        if (minPosition > maxPosition)
            return false;

        var position = Main.Count - footer.Top(2);
        bool fit = position <= maxPosition;
        position = Math.Clamp(position, minPosition, maxPosition);

        columns[0, 1] = '_';
        columns[0, 2] = '$';
        columns.Write(0, 3, Dir.Down, FalseBranch);
        columns[1, 0] = '_';
        columns.Write(1, 1, Dir.Down, TrueBranch);

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
        var footer = UpwardFooter;

        int minPosition = footer.MinPosition(3 + FalseBranch.Count, 1 + TrueBranch.Count, 0);
        int maxPosition = footer.MaxPosition(columns.Height(0), columns.Height(1), columns.Height(2) - 1);

        if (minPosition > maxPosition)
            return false;

        columns[0, 1] = '_';
        columns[0, 2] = '$';
        columns.Write(0, 3, Dir.Down, FalseBranch);
        columns[1, 0] = '_';
        columns.Write(1, 1, Dir.Down, TrueBranch);

        footer.Write(columns, minPosition);

        var main = new CommandsEnumerator(Main);
        int mainStart = minPosition + footer.Bottom(2);
        int height = Math.Clamp(main.Count + mainStart + 4 >> 1, mainStart + 1, columns.Height(2));

        columns.Write(2, mainStart, Dir.Down, main, height - mainStart - 1);
        columns[2, height - 1] = '_';
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
