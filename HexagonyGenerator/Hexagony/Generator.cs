namespace HexagonyGenerator.Hexagony;

using Bytecode;

/*
          / \ . . . . . . . .  (bottom)

          / ) _ . . _ . . . .
         > ~ < > ) < > ) < . .
        \ < $ . < $ . < $ . . .
       . . . _ a . _ a . _ a . .
      . . . _ b a _ b a _ b a . .
     . . . . $ b a $ b a $ b a . .
    . . . . . c b a c b a c b a . .
   . . . . . . c b c c b a c b a . .
  . . . . . . . . . . . . . . . . . .

*/

class Generator
{
    private readonly Hexagon hxg = new();

    private Generator() { }

    public static Hexagon Generate(Program program)
    {
        Generator generator = new();
        generator.WriteProgram(program);
        return generator.hxg;
    }

    private void WriteProgram(Program program)
    {
        foreach (var procedure in program.Procedures)
            WriteProcedure(procedure);

        hxg[0, 0] = '/';
        hxg[0, 1] = ')';
        hxg[1, 0] = '~';
        hxg[2, -2] = '\\';

        int size = hxg.Size;
        hxg[2 * size, -size] = '/';
        hxg[2 * size, -size + 1] = '\\';
    }

    /*
    
    FOOTERS

    aaa - entry
    bbb - true branch
    ccc - false branch

    true      x > 0     x < 0     x != 0

    . b a     c b a     c b a     c b a
     . b a     c b a     c b a     c b a
      . b a     c b a     c ~ ~     c b a
       . b |     c > /     ~ > /     c b $
        . _ .     _ . .     _ . .     c > <
         . . .     . . .     . . .     _ $ .
                                        . ~ ~
                                         . > /
                                          > . .
                                           . . .

    */

    private struct ColumnsTriple
    {
        private readonly Commands[] _triple;

        public ColumnsTriple(Commands main, Commands trueBranch, Commands falseBranch)
        {
            _triple = new[] { main, trueBranch, falseBranch };
        }

        public ColumnsTriple(string main, string trueBranch, string falseBranch)
            : this(Convert(main), Convert(trueBranch), Convert(falseBranch))
        {
        }

        public Commands this[int index] => _triple[index];

        private static Commands Convert(string text)
        {
            Commands commands = new();
            foreach (var c in text)
                commands.Add(c);
            return commands;
        }
    }

    private static readonly ColumnsTriple
        Header = new("<.", ")$_", "><._$"),
        TrueFooter = new(".|", "_", ""),
        PositiveFooter = new("./", ".>", "_"),
        NegativeFooter = new("./~", ".>~", "_~"),
        NonzeroFooter = new("./~.<$", ".>~$>", ">.._");

    private void WriteProcedure(Procedure procedure)
    {
        int mainY = 3 * procedure.Index + 1;

        if (procedure.Index > 0) hxg[0, mainY - 2] = '_';

        GenerateCommands(procedure, out var body, out var footer);

        var lastX = Enumerable.Range(0, 3).Max(i =>
        {
            int x = 1;
            int y = mainY - i;
            foreach (var cmd in Header[i].Concat(body[i]))
                hxg[x++, y] = cmd;
            return x + footer[i].Count;
        }) - 1;

        for (int i = 0; i < 3; i++)
        {
            int x = lastX;
            int y = mainY - i;
            foreach (var cmd in footer[i])
                hxg[x--, y] = cmd;
        }
    }

    private static void GenerateCommands(Procedure procedure, out ColumnsTriple body, out ColumnsTriple footer)
    {
        var main = new Commands();
        var memory = new Memory(main);
        foreach (var action in procedure.Actions)
            action.ApplyTo(memory);

        Commands SetIp(Procedure procedure)
        {
            Commands cmds = new();
            if (procedure != Procedure.Exit)
            {
                new Memory(cmds, memory).SetIp(procedure.Index - 1);
                cmds.Reverse();
            }
            else
                cmds.Add(Command.Exit);
            return cmds;
        }

        switch (procedure.Continuation)
        {
            case ConditionalContinuation continuation:
                memory.MoveTo(continuation.ConditionVar);
                body = new(main, SetIp(continuation.TrueBranch), SetIp(continuation.FalseBranch));
                footer = continuation.Type switch
                {
                    ConditionType.Positive => PositiveFooter,
                    ConditionType.Negative => NegativeFooter,
                    ConditionType.Nonzero => NonzeroFooter,
                    _ => throw new UnexpectedDefaultException(),
                };
                break;
            case Continuation continuation:
                body = new(main, SetIp(continuation.Next), Commands.Empty);
                footer = TrueFooter;
                break;
            default:
                throw new UnexpectedDefaultException();
        }
    }
}
