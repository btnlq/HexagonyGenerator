namespace HexagonyGenerator.Bytecode;

class Variable : ISymbol
{
    public /*readonly*/ int Location;

    public Variable(int location)
    {
        Location = location;
    }
}
