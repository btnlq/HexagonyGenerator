namespace HexagonyGenerator.Bytecode;

// IAction = Assignment | ConditionalAssignment | Writing | Reading
interface IAction
{
    void ApplyTo(IMemory memory);
}
