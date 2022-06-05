namespace HexagonyGenerator.SourceCode.Parser;

enum TokenType
{
    LBrace,
    RBrace,
    LParen,
    RParen,
    Comma,
    Semicolon,
    Eof,

    Identifier,
    Keyword,
    Integer,
    Character,
    String,
    ArithmeticOperator,
    BooleanOperator,
    Assignment,
    IncDec,
    ComparisonOperator,

    Invalid,
}

class Token
{
    public readonly TokenType Type;
    public readonly int LineId;
    public readonly int Column;
    public readonly string Text;

    public Token(TokenType type, int lineId, int column, string text)
    {
        Type = type;
        LineId = lineId;
        Column = column;
        Text = text;
    }
}
