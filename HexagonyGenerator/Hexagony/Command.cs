namespace HexagonyGenerator.Hexagony;

static class Command
{
    // Special
    public const char Nop = '.';
    public const char Exit = '@';

    // Arithmetic
    public const char Mul10 = '0';
    public const char Increment = ')';
    public const char Decrement = '(';
    public const char Add = '+';
    public const char Sub = '-';
    public const char Mul = '*';
    public const char Div = ':';
    public const char Mod = '%';
    public const char Negate = '~';

    // I/O
    public const char ReadByte = ',';
    public const char ReadInt = '?';
    public const char WriteByte = ';';
    public const char WriteInt = '!';

    // Memory manipulation
    public const char TurnLeft = '{';
    public const char TurnRight = '}';
    public const char TurnLeftBackwards = '"';
    public const char TurnRightBackwards = '\'';
    public const char ReverseMp = '=';
    public const char Copy = '&';
}
