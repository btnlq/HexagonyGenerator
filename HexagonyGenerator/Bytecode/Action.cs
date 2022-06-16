namespace HexagonyGenerator.Bytecode;

// IAction = Assignment | Writing | Reading
interface IAction
{
    void ApplyTo(IMemory memory);
}
