namespace HexagonyGenerator.SourceCode;

class SimpleActionList
{
    private readonly List<ISimpleAction> _statements = new();

    public IEnumerable<IStatement> Statements => _statements;

    public SimpleActionList() { }

    public void Add<Action>(Action action) where Action : Bytecode.IAction
        => _statements.Add(action.AsStatement());
}
