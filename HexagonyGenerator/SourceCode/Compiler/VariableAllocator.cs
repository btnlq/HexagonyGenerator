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

    private void ScanBlock(Block block)
    {
        foreach (var statement in block)
        {
            switch (statement)
            {
                case SimpleAction<Bytecode.Assignment> action:
                {
                    var assignment = action.Action;
                    if (assignment.Left is Bytecode.Variable left)
                        Put(left);
                    if (assignment.Right is Bytecode.Variable right)
                        Put(right);
                    _index++;
                    Put(assignment.Dest);
                    _index++;
                    break;
                }
                case SimpleAction<Bytecode.Writing> action:
                {
                    if (action.Action.Symbol is Bytecode.Variable variable)
                        Put(variable);
                    _index++;
                    break;
                }
                case Loop loop:
                {
                    int start = _index;
                    ScanBlock(loop.Block);
                    foreach (var scope in _scopes)
                        if (scope.Start < start && scope.End >= start)
                            scope.End = _index;
                    break;
                }
                case Conditional conditional:
                {
                    Put(conditional.Variable);
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
