namespace HexagonyGenerator.SourceCode.Parser;

class ParserException : CompilationException
{
    public ParserException(string message, int lineId, int column)
        : base($"Parser error at line {lineId + 1} column {column + 1}. {message}")
    {
    }

    public ParserException(string message, Token token)
        : this(message, token.LineId, token.Column)
    {
    }
}
