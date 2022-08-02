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

    private static List<Procedure> OptimizeContinuations(Procedure start)
    {
        List<Procedure> toOptimize = new();

        void Add(Procedure procedure)
        {
            if (procedure != Procedure.Exit && procedure.Index == Procedure.NotIndexed)
            {
                procedure.Index = toOptimize.Count;
                toOptimize.Add(procedure);
            }
        }

        Add(start);

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
                procedure.Continuation = conditionalContinuation =
                    new ConditionalContinuation(
                        conditionalContinuation.ConditionSymbol, conditionalContinuation.Type, trueBranch, falseBranch);
            Add(conditionalContinuation.TrueBranch);
            Add(conditionalContinuation.FalseBranch);

            // TODO: check if TrueBranch == FalseBranch
        }

        if (toOptimize.Count == 0)
            toOptimize.Add(Procedure.Exit);
        return toOptimize;
    }

    private static Procedure OptimizeStart(Procedure start)
    {
        while (start.IsEmpty())
        {
            Visitor.Visit(start.Continuation!);
            start = start.Continuation switch
            {
                Continuation continuation => continuation.Next,
                ConditionalContinuation continuation => continuation.FalseBranch,
                _ => throw new UnexpectedDefaultException(),
            };
        }
        Visitor.Clear();
        return start;
    }

    public static Program Optimize(Procedure start)
        => new(OptimizeContinuations(OptimizeStart(start)));
}
