namespace HexagonyGenerator.SourceCode.Compiler;

using Bytecode;

static class ContinuationsOptimizer
{
    private static class Visitor
    {
        private static readonly HashSet<IContinuation> _visited = new();

        public static void Visit(IContinuation continuation)
        {
            if (!_visited.Add(continuation))
                throw new CompilationException("Infinite loop detected");
        }

        public static void Clear() => _visited.Clear();
    }

    private static bool IsEmpty(this Procedure procedure) =>
        procedure.Actions.Count == 0 && procedure != Procedure.Exit;

    private static bool SkipEmpty(Procedure procedure, out Procedure next)
    {
        next = procedure;
        while (next.IsEmpty() && next.Continuation is Continuation continuation)
        {
            Visitor.Visit(continuation);
            next = continuation.Next;
        }
        Visitor.Clear();
        return next != procedure;
    }

    private static void OptimizeContinuations(Procedure start)
    {
        List<Procedure> toOptimize = new() { start };

        void Add(Procedure procedure)
        {
            if (procedure != Procedure.Exit && !toOptimize.Contains(procedure))
                toOptimize.Add(procedure);
        }

        for (int i = 0; i < toOptimize.Count; i++)
        {
            var procedure = toOptimize[i];

            if (procedure.Continuation is Continuation continuation)
            {
                if (SkipEmpty(continuation.Next, out var next))
                    continuation = new Continuation(next);

                if (!continuation.Next.IsEmpty())
                {
                    procedure.Continuation = continuation;
                    Add(continuation.Next);
                    continue;
                }
                procedure.Continuation = continuation.Next.Continuation;
            }

            var conditionalContinuation = (ConditionalContinuation)procedure.Continuation!;

            if (SkipEmpty(conditionalContinuation.TrueBranch, out var trueBranch) |
                SkipEmpty(conditionalContinuation.FalseBranch, out var falseBranch))
                procedure.Continuation =
                    new ConditionalContinuation(
                        conditionalContinuation.ConditionVar, conditionalContinuation.Type, trueBranch, falseBranch);
            Add(conditionalContinuation.TrueBranch);
            Add(conditionalContinuation.FalseBranch);

            // TODO: check if TrueBranch == FalseBranch
        }
    }

    private static void OptimizeStart(Program program)
    {
        while (program.Start.IsEmpty())
        {
            Visitor.Visit(program.Start.Continuation!);
            program.Start = program.Start.Continuation switch
            {
                Continuation continuation => continuation.Next,
                ConditionalContinuation continuation => continuation.FalseBranch,
                _ => throw new UnexpectedDefaultException(),
            };
        }
        Visitor.Clear();
    }

    public static void Optimize(Program program)
    {
        OptimizeStart(program);
        if (program.Start == Procedure.Exit)
            return;
        OptimizeContinuations(program.Start);
    }
}
