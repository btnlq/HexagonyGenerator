namespace HexagonyGenerator.Bytecode.Interpreter;

class Executor
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
                var value = memory[continuation.ConditionVar];
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
        {
            var comparer = Comparer<Procedure>.Create((p1, p2) => calls[p2.Index].CompareTo(calls[p1.Index]));
            int length = program.Procedures.Count;
            program.Procedures.Sort(1, length - 1, comparer);
            for (int i = 1; i < length; i++)
                program.Procedures[i].Index = i;
        }

        return memory.GetOutput();
    }
}
