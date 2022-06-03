namespace HexagonyGenerator.Bytecode.Interpreter;

class Executor
{
    public static string Execute(Program program, Hexagony.Interpreter.Reader reader)
    {
        var procedure = program.Start;
        Memory memory = new(reader);

        while (procedure != Procedure.Exit)
        {
            foreach (var action in procedure.Actions)
                action.ApplyTo(memory);

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
        }

        return memory.GetOutput();
    }
}
