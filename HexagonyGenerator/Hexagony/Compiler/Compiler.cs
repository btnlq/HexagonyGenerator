namespace HexagonyGenerator.Hexagony.Compiler;

static class Compiler
{
    public static Procedure Compile(Bytecode.Procedure procedure)
    {
        var main = new Commands();
        var memory = new Memory(main);
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
            case Bytecode.ConditionalContinuation continuation:
                memory.MoveTo(continuation.ConditionVar);
                return new(main,
                    SetIp(continuation.TrueBranch),
                    SetIp(continuation.FalseBranch),
                    continuation.Type);
            case Bytecode.Continuation continuation:
                SetIp(continuation.Next, main);
                return new(main);
            default:
                throw new UnexpectedDefaultException();
        }
    }
}
