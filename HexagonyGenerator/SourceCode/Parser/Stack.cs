namespace HexagonyGenerator.SourceCode.Parser;

using Variable = Bytecode.Variable;

class Stack
{
    private class Frame
    {
        public readonly string? Name;
        public readonly Block Block;
        public readonly Dictionary<string, Variable> Variables = new();

        public Frame(string? name, Block block)
        {
            Name = name;
            Block = block;
        }
    }

    private readonly Stack<Frame> _frames = new();

    public void PushFrame(Block block, string? blockName)
    {
        _frames.Push(new(blockName, block));
    }

    public void PopFrame()
    {
        _frames.Pop();
    }

    public Variable? GetVariable(string variableName, bool create = false)
    {
        foreach (var frame in _frames)
            if (frame.Variables.TryGetValue(variableName, out var variable))
                return variable;

        if (create)
        {
            var variable = Compiler.VariableAllocator.New();
            _frames.Peek().Variables.Add(variableName, variable);
            return variable;
        }

        return null;
    }

    public Block? GetBlock(string blockName)
    {
        foreach (var frame in _frames)
            if (frame.Name == blockName)
                return frame.Block;

        return null;
    }

    public Block? GetTopNamedBlock()
    {
        foreach (var frame in _frames)
            if (frame.Name != null)
                return frame.Block;

        return null;
    }
}
