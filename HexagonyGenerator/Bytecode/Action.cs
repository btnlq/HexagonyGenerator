namespace HexagonyGenerator.Bytecode;

// IAction = Assignment | Writing
interface IAction
{
    void ApplyTo(IMemory memory);
}
