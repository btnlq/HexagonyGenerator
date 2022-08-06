namespace HexagonyGenerator.Hexagony;

static class PrimitiveGenerator
{
    private static Commands Compile(Bytecode.Program program)
    {
        var commands = new Commands();
        var memory = new Compiler.Memory(commands, Compiler.Compiler.NoInput(program.Start));
        foreach (var action in program.Start.Actions)
            action.ApplyTo(memory);
        return commands;
    }

    private static Hexagon Draw(Commands commands)
    {
        int size = 2;
        while (size * (3 * size - 2) < commands.Count)
            size++;

        Hexagon hxg = new(size);

        int x = 0, y = 0, dir = 1;

        foreach (int cmd in commands)
        {
            if (dir == 1)
            {
                if (y == size || y == 2 * size - x)
                {
                    hxg[x++, y--] = '\\';
                    hxg[x, y--] = '<';
                    dir = -1;
                }
            }
            else
            {
                if (y == -size || y == -x)
                {
                    hxg[x++, y] = '/';
                    hxg[x, y++] = '>';
                    dir = 1;
                }
            }

            hxg[x, y] = cmd;
            y += dir;
        }

        hxg[x, y] = Command.Exit;

        return hxg;
    }

    public static Hexagon? Generate(Bytecode.Program program)
    {
        if (!(program.Start.Continuation is Bytecode.Continuation continuation && continuation.Next == Bytecode.Procedure.Exit))
            return null;

        return Draw(Compile(program));
    }
}
