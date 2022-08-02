namespace HexagonyGenerator.Bytecode.Interpreter;

static class Executor
{
    public static string Execute(Program program, Hexagony.Interpreter.Reader reader, bool optimize, out int executed)
    {
        var procedure = program.Start;
        Memory memory = new(reader);

        executed = 0;
        int[] calls = optimize ? new int[program.Procedures.Count] : null!;

        while (procedure != Procedure.Exit)
        {
            if (optimize)
                calls[procedure.Index]++;

            foreach (var action in procedure.Actions)
            {
                action.ApplyTo(memory);
                executed++;
            }

            if (procedure.Continuation is ConditionalContinuation continuation)
            {
                var value = memory.Get(continuation.ConditionSymbol);
                bool isTrue = continuation.Type switch
                {
                    ConditionType.Positive => value > 0,
                    ConditionType.Negative => value < 0,
                    ConditionType.Nonzero => value != 0,
                    _ => throw new UnexpectedDefaultException(),
                };
                procedure = isTrue ? continuation.TrueBranch : continuation.FalseBranch;
            }
            else
            {
                procedure = ((Continuation)procedure.Continuation!).Next;
            }

            executed++;
        }

        if (optimize)
            program.OrderByDescending(calls);

        return memory.GetOutput();
    }
}
