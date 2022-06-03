namespace HexagonyGenerator.SourceCode.Parser;

static class Keyword
{
    public const string While = "while";
    public const string For = "for";
    public const string Break = "break";
    public const string Continue = "continue";
    public const string Exit = "exit";
    public const string If = "if";
    public const string Else = "else";
    public const string ReadByte = "get";
    public const string WriteByte = "put";
    public const string ReadInt = "read";
    public const string WriteInt = "write";
    public const string WritelnInt = "writeln";

    public static bool Is(string str) => str is While or For or Break or Continue or Exit
        or If or Else or ReadByte or WriteByte or ReadInt or WriteInt or WritelnInt;
}
