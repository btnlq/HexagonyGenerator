namespace HexagonyGenerator.Bytecode;

class Variable
{
    public /*readonly*/ int Location;

    public Variable(int location)
    {
        Location = location;
    }

    public bool Is(Variable other) => Location == other.Location;
}
