namespace HexagonyGenerator.SourceCode.Compiler;

class VariableAllocator
{
    private const int NotAllocated = int.MinValue;

    public static Bytecode.Variable New() => new(NotAllocated);

    public static void Allocate(Program program) => new VariableAllocator().AllocateInternal(program);

    private int _index;

    private class Scope
    {
        public Bytecode.Variable Variable;
        public int Start, End;

        public Scope(Bytecode.Variable variable, int start, int end)
        {
            Variable = variable;
            Start = start;
            End = end;
        }
    }

    private readonly List<Scope> _scopes = new();

    private void Put(Bytecode.Variable variable)
    {
        if (variable.Location == NotAllocated)
        {
            variable.Location = _scopes.Count;
            _scopes.Add(new Scope(variable, _index, _index));
        }
        else
            _scopes[variable.Location].End = _index;
    }

    private void TryPut(Bytecode.ISymbol? symbol)
    {
        if (symbol is Bytecode.VariableSymbol variable)
            Put(variable.Variable);
    }

    private void ScanBlock(Block block)
    {
        foreach (var statement in block)
        {
            switch (statement)
            {
                case SimpleAction<Bytecode.Assignment> action:
                {
                    var assignment = action.Action;
                    TryPut(assignment.Left);
                    TryPut(assignment.Right);
                    _index++;
                    Put(assignment.Dest);
                    _index++;
                    break;
                }
                case SimpleAction<Bytecode.Writing> action:
                {
                    TryPut(action.Action.Symbol);
                    _index++;
                    break;
                }
                case BlockStatement blockStmt:
                {
                    int start = _index;
                    ScanBlock(blockStmt.Block);
                    if (blockStmt.Block.IsLoop != null)
                        foreach (var scope in _scopes)
                            if (scope.Start < start && scope.End >= start)
                                scope.End = _index;
                    break;
                }
                case Conditional conditional:
                {
                    TryPut(conditional.Symbol);
                    _index++;
                    if (conditional.TrueBranch != null)
                        ScanBlock(conditional.TrueBranch);
                    if (conditional.FalseBranch != null)
                        ScanBlock(conditional.FalseBranch);
                    break;
                }
                case Goto:
                case Exit:
                    continue;
                case SimpleAction<Bytecode.Reading>:
                    break;
                default:
                    throw new UnexpectedDefaultException();
            }
        }
    }

    private void AllocateInternal(Program program)
    {
        ScanBlock(program.Block);

        List<int> ends = new();
        foreach (var scope in _scopes)
        {
            int location = ends.FindIndex(end => scope.Start > end);
            if (location < 0)
            {
                location = ends.Count;
                ends.Add(0);
            }
            scope.Variable.Location = location;
            ends[location] = scope.End;
        }
    }
}
