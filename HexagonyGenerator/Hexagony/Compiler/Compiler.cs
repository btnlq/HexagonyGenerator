namespace HexagonyGenerator.Hexagony.Compiler;

static class Compiler
{
    public static Procedure Compile(Bytecode.Procedure procedure, bool noInput)
    {
        var main = new Commands();
        var memory = new Memory(main, noInput);
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
                memory.SetAny(continuation.ConditionSymbol);
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

    private class InputDetector : Bytecode.IMemory
    {
        public bool HasInput;

        private void Check(Bytecode.ISymbol symbol)
        {
            if (symbol is Bytecode.ReadingSymbol)
                HasInput = true;
        }

        public void Read(Bytecode.VariableType type) => HasInput = true;

        public void Set(Bytecode.Variable dest, Bytecode.ISymbol value) => Check(value);

        public void Set(Bytecode.Variable dest, Bytecode.ISymbol left, Bytecode.BinOp op, Bytecode.ISymbol right)
        {
            Check(left);
            Check(right);
        }

        public void Set(Bytecode.Variable dest, Bytecode.ModifiableSymbol conditionSymbol, Bytecode.ISymbol trueValue, Bytecode.ISymbol falseValue)
        {
            Check(conditionSymbol);
            Check(trueValue);
            Check(falseValue);
        }

        public void Write(Bytecode.ISymbol symbol, Bytecode.VariableType type) => Check(symbol);
    }

    public static bool NoInput(Bytecode.Procedure procedure)
    {
        var detector = new InputDetector();
        foreach (var action in procedure.Actions)
        {
            action.ApplyTo(detector);
            if (detector.HasInput)
                return false;
        }
        return true;
    }
}
