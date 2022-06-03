namespace HexagonyGenerator.SourceCode;

interface ISimpleAction : IStatement
{
    Bytecode.IAction Action { get; }
}

class SimpleAction<IAction> : ISimpleAction where IAction : Bytecode.IAction
{
    public readonly IAction Action;

    Bytecode.IAction ISimpleAction.Action => Action;

    public SimpleAction(IAction action)
    {
        Action = action;
    }
}

static class SimpleActionEx
{
    public static SimpleAction<IAction> AsStatement<IAction>(this IAction action)
        where IAction : Bytecode.IAction => new(action);
}
